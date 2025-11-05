# Code Signing and Deployment Security

## Overview

This document covers code signing, secure deployment practices, and supply chain security for Windows desktop applications.

## Table of Contents

1. [Code Signing Overview](#code-signing-overview)
2. [Obtaining Code Signing Certificates](#obtaining-code-signing-certificates)
3. [Signing Applications](#signing-applications)
4. [Secure Build Pipeline](#secure-build-pipeline)
5. [Package Distribution Security](#package-distribution-security)
6. [Secure Update Mechanism](#secure-update-mechanism)
7. [Supply Chain Security](#supply-chain-security)

---

## Code Signing Overview

### Why Code Signing Matters

Code signing provides:
- **Identity verification** - Proves the software comes from you
- **Integrity assurance** - Confirms the code hasn't been tampered with
- **Trust** - Windows SmartScreen and users trust signed applications
- **Compliance** - Required for some distribution channels (Windows Store, enterprise deployments)

### Code Signing Certificate Types

**Standard Code Signing Certificate**
- Basic validation (email verification)
- Requires timestamp server
- Less expensive
- Not recommended for production

**Extended Validation (EV) Code Signing Certificate**
- Hardware token or cloud-based HSM required
- Immediate SmartScreen reputation
- More expensive
- **Recommended for production applications**

---

## Obtaining Code Signing Certificates

### Certificate Authorities

Trusted CAs for Windows code signing:
- DigiCert
- Sectigo (formerly Comodo)
- GlobalSign
- Entrust
- SSL.com

### Acquisition Process

1. **Choose Certificate Type**
   - EV certificate for production (recommended)
   - Standard certificate for development/testing

2. **Business Verification**
   - Company documents (articles of incorporation, etc.)
   - D-U-N-S number (for EV certificates)
   - Phone verification

3. **Certificate Issuance**
   - EV: Hardware token or cloud HSM
   - Standard: PFX file with password

### Cost Considerations

- **Standard:** $100-300/year
- **EV:** $300-500/year
- **Cloud HSM:** Additional $10-50/month

### Development/Testing Certificates

For development, create self-signed certificates:

```powershell
# Create self-signed certificate for testing
$cert = New-SelfSignedCertificate `
    -Type CodeSigning `
    -Subject "CN=MyCompany Development" `
    -KeyUsage DigitalSignature `
    -FriendlyName "MyApp Development Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3") `
    -NotAfter (Get-Date).AddYears(3)

# Export certificate
$certPassword = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "MyAppDevCert.pfx" -Password $certPassword
```

**⚠️ Warning:** Self-signed certificates trigger security warnings and should never be used for production.

---

## Signing Applications

### Using SignTool

SignTool is included with the Windows SDK.

**Basic Signing:**
```bash
# Sign with PFX file
signtool sign /f "certificate.pfx" /p "password" /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 MyApp.exe

# Sign with certificate from store
signtool sign /n "Certificate Subject Name" /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 MyApp.exe
```

**Parameters Explained:**
- `/f` - Certificate file path
- `/p` - Certificate password
- `/n` - Certificate subject name (from cert store)
- `/fd` - File digest algorithm (SHA256)
- `/tr` - Timestamp server URL
- `/td` - Timestamp digest algorithm (SHA256)

### Timestamp Servers

Always use timestamping to ensure signatures remain valid after certificate expiration:

```bash
# DigiCert
http://timestamp.digicert.com

# Sectigo
http://timestamp.sectigo.com

# GlobalSign
http://timestamp.globalsign.com
```

### Signing Multiple Files

```bash
# Sign all EXE and DLL files
Get-ChildItem -Path ".\publish" -Recurse -Include *.exe,*.dll | ForEach-Object {
    signtool sign /f "certificate.pfx" /p "password" /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 $_.FullName
}
```

### Signing in MSBuild

Add to `.csproj`:

```xml
<Project>
  <PropertyGroup>
    <SignAssembly>true</SignAssembly>
    <SigningCertificate>$(CertificatePath)</SigningCertificate>
    <SigningCertificatePassword>$(CertificatePassword)</SigningCertificatePassword>
  </PropertyGroup>
  
  <Target Name="SignOutput" AfterTargets="Build">
    <Exec Command="signtool sign /f &quot;$(SigningCertificate)&quot; /p &quot;$(SigningCertificatePassword)&quot; /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 &quot;$(TargetPath)&quot;" />
  </Target>
</Project>
```

### Verifying Signatures

```bash
# Verify signature
signtool verify /pa /v MyApp.exe

# Check timestamp
signtool verify /pa /v /tw MyApp.exe
```

---

## Secure Build Pipeline

### GitHub Actions Example

```yaml
name: Build and Sign

on:
  push:
    tags:
      - 'v*'

jobs:
  build-and-sign:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --configuration Release --no-build
    
    - name: Publish
      run: dotnet publish --configuration Release --output ./publish
    
    - name: Import Certificate
      shell: powershell
      run: |
        $cert = [Convert]::FromBase64String("${{ secrets.CERTIFICATE_BASE64 }}")
        [IO.File]::WriteAllBytes("cert.pfx", $cert)
    
    - name: Sign Application
      shell: powershell
      run: |
        $certPassword = "${{ secrets.CERTIFICATE_PASSWORD }}"
        Get-ChildItem -Path "./publish" -Recurse -Include *.exe,*.dll | ForEach-Object {
          & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe" sign /f cert.pfx /p $certPassword /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 $_.FullName
        }
    
    - name: Remove Certificate
      if: always()
      run: Remove-Item cert.pfx -Force
    
    - name: Create Installer
      run: iscc installer.iss
    
    - name: Sign Installer
      shell: powershell
      run: |
        $cert = [Convert]::FromBase64String("${{ secrets.CERTIFICATE_BASE64 }}")
        [IO.File]::WriteAllBytes("cert.pfx", $cert)
        $certPassword = "${{ secrets.CERTIFICATE_PASSWORD }}"
        & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe" sign /f cert.pfx /p $certPassword /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 "Output\MyAppSetup.exe"
        Remove-Item cert.pfx -Force
    
    - name: Upload Artifacts
      uses: actions/upload-artifact@v3
      with:
        name: signed-application
        path: |
          ./publish/**/*
          Output/MyAppSetup.exe
```

### Azure Pipelines Example

```yaml
trigger:
  tags:
    include:
    - v*

pool:
  vmImage: 'windows-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: DotNetCoreCLI@2
  displayName: 'Restore'
  inputs:
    command: 'restore'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Test'
  inputs:
    command: 'test'
    arguments: '--configuration $(buildConfiguration) --no-build'

- task: DotNetCoreCLI@2
  displayName: 'Publish'
  inputs:
    command: 'publish'
    publishWebProjects: false
    arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)/publish'

- task: AzureKeyVault@2
  displayName: 'Get Certificate from Key Vault'
  inputs:
    azureSubscription: 'MyAzureSubscription'
    KeyVaultName: 'mykeyvault'
    SecretsFilter: 'CodeSigningCertificate'

- task: PowerShell@2
  displayName: 'Sign Binaries'
  inputs:
    targetType: 'inline'
    script: |
      $certBytes = [Convert]::FromBase64String("$(CodeSigningCertificate)")
      $certPath = Join-Path $env:TEMP "cert.pfx"
      [IO.File]::WriteAllBytes($certPath, $certBytes)
      
      Get-ChildItem -Path "$(Build.ArtifactStagingDirectory)/publish" -Recurse -Include *.exe,*.dll | ForEach-Object {
        & signtool sign /f $certPath /p "$(CertificatePassword)" /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 $_.FullName
      }
      
      Remove-Item $certPath -Force

- task: PublishBuildArtifacts@1
  displayName: 'Publish Artifacts'
  inputs:
    pathToPublish: '$(Build.ArtifactStagingDirectory)'
    artifactName: 'signed-binaries'
```

### Certificate Storage Best Practices

**DO:**
- ✅ Store certificates in Azure Key Vault or similar HSM
- ✅ Use environment variables for passwords
- ✅ Rotate certificates before expiration
- ✅ Use EV certificates for production
- ✅ Implement certificate pinning in auto-update mechanism

**DON'T:**
- ❌ Commit certificates to source control
- ❌ Store certificate passwords in plain text
- ❌ Share certificates between projects
- ❌ Use expired certificates
- ❌ Skip timestamping

---

## Package Distribution Security

### Installer Security

**Inno Setup Security Configuration:**

```iss
[Setup]
AppName=MyApplication
AppVersion=1.0.0
DefaultDirName={autopf}\MyApplication
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=MyAppSetup
Compression=lzma2
SolidCompression=yes
; Security settings
PrivilegesRequired=admin
SignTool=signtool
SignedUninstaller=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Run]
Filename: "{app}\MyApp.exe"; Description: "Launch MyApplication"; Flags: postinstall nowait skipifsilent

[Code]
// Verify signature before installation
function VerifyExecutableSignature(FileName: String): Boolean;
var
  ResultCode: Integer;
begin
  Exec('signtool', 'verify /pa "' + FileName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not VerifyExecutableSignature(ExpandConstant('{srcexe}')) then
  begin
    MsgBox('Installer signature verification failed!', mbError, MB_OK);
    Result := False;
  end;
end;
```

### MSI Packages

```powershell
# Sign MSI package
signtool sign /f "certificate.pfx" /p "password" /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 MyApp.msi

# Verify MSI signature
signtool verify /pa /v MyApp.msi
```

### Windows Store Distribution

For Microsoft Store:
1. Applications are automatically signed by Microsoft
2. Implement app certificate pinning
3. Use Microsoft Store Services SDK for license validation

---

## Secure Update Mechanism

### Update Architecture

```
┌─────────────┐                    ┌─────────────┐
│   Client    │                    │   Server    │
│             │                    │             │
│  1. Check   ├───────────────────>│  Manifest   │
│     Update  │                    │   (Signed)  │
│             │<───────────────────┤             │
│             │   Manifest+Sig     │             │
│             │                    │             │
│  2. Verify  │                    │             │
│  Signature  │                    │             │
│             │                    │             │
│  3. Download├───────────────────>│  Package    │
│    Package  │                    │   (Signed)  │
│             │<───────────────────┤             │
│             │   Package+Sig      │             │
│             │                    │             │
│  4. Verify  │                    │             │
│  & Install  │                    │             │
└─────────────┘                    └─────────────┘
```

### Implementation

```csharp
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class SecureUpdateManager
{
    private readonly string _updateServerUrl;
    private readonly string _certificateThumbprint;
    
    public SecureUpdateManager(string updateServerUrl, string certificateThumbprint)
    {
        _updateServerUrl = updateServerUrl;
        _certificateThumbprint = certificateThumbprint;
    }
    
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            // Download manifest
            using var client = new HttpClient();
            var manifestUrl = $"{_updateServerUrl}/manifest.json";
            var manifestContent = await client.GetStringAsync(manifestUrl);
            
            // Download signature
            var signatureUrl = $"{_updateServerUrl}/manifest.json.sig";
            var signatureBytes = await client.GetByteArrayAsync(signatureUrl);
            
            // Verify signature
            if (!VerifySignature(manifestContent, signatureBytes))
            {
                Logger.Error("Update manifest signature verification failed");
                return null;
            }
            
            // Parse manifest
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(manifestContent);
            
            if (manifest != null && IsNewerVersion(manifest.Version))
            {
                return new UpdateInfo
                {
                    Version = manifest.Version,
                    DownloadUrl = manifest.DownloadUrl,
                    ReleaseNotes = manifest.ReleaseNotes
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error($"Update check failed: {ex.Message}");
            return null;
        }
    }
    
    public async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo update)
    {
        try
        {
            // Download update package
            using var client = new HttpClient();
            var packageBytes = await client.GetByteArrayAsync(update.DownloadUrl);
            
            // Download signature
            var signatureBytes = await client.GetByteArrayAsync($"{update.DownloadUrl}.sig");
            
            // Verify package signature
            if (!VerifySignature(packageBytes, signatureBytes))
            {
                Logger.Error("Update package signature verification failed");
                return false;
            }
            
            // Save to temp file
            var tempFile = Path.Combine(Path.GetTempPath(), "update.exe");
            await File.WriteAllBytesAsync(tempFile, packageBytes);
            
            // Verify code signature
            if (!VerifyCodeSignature(tempFile))
            {
                Logger.Error("Update package code signature verification failed");
                File.Delete(tempFile);
                return false;
            }
            
            // Launch installer
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true,
                Verb = "runas" // Request elevation
            });
            
            // Exit application to allow update
            Environment.Exit(0);
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Update installation failed: {ex.Message}");
            return false;
        }
    }
    
    private bool VerifySignature(string content, byte[] signature)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        return VerifySignature(contentBytes, signature);
    }
    
    private bool VerifySignature(byte[] data, byte[] signature)
    {
        try
        {
            // Load certificate from store
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            
            var cert = store.Certificates
                .Find(X509FindType.FindByThumbprint, _certificateThumbprint, false)
                .Cast<X509Certificate2>()
                .FirstOrDefault();
            
            if (cert == null)
            {
                Logger.Error("Certificate not found in store");
                return false;
            }
            
            using var rsa = cert.GetRSAPublicKey();
            if (rsa == null)
                return false;
            
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex)
        {
            Logger.Error($"Signature verification failed: {ex.Message}");
            return false;
        }
    }
    
    private bool VerifyCodeSignature(string filePath)
    {
        try
        {
            var cert = X509Certificate.CreateFromSignedFile(filePath);
            var cert2 = new X509Certificate2(cert);
            
            // Verify certificate is valid and trusted
            var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            
            return chain.Build(cert2);
        }
        catch (Exception ex)
        {
            Logger.Error($"Code signature verification failed: {ex.Message}");
            return false;
        }
    }
    
    private bool IsNewerVersion(string remoteVersion)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var remote = Version.Parse(remoteVersion);
        return remote > currentVersion;
    }
}

public class UpdateManifest
{
    public string Version { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string ReleaseNotes { get; set; } = "";
    public string Sha256Hash { get; set; } = "";
}

public class UpdateInfo
{
    public string Version { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string ReleaseNotes { get; set; } = "";
}
```

---

## Supply Chain Security

### Dependency Management

**Verify Package Integrity:**

```xml
<!-- .csproj -->
<PropertyGroup>
  <NuGetAudit>true</NuGetAudit>
  <NuGetAuditLevel>low</NuGetAuditLevel>
  <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
  <RestoreLockedMode>true</RestoreLockedMode>
</PropertyGroup>
```

### Secure Development Practices

1. **Code Review**
   - All code changes reviewed before merge
   - Security-focused reviews for sensitive areas
   - Automated security scanning in CI/CD

2. **Access Control**
   - Limit who can merge to main branch
   - Require 2FA for all developers
   - Use branch protection rules

3. **Build Environment Security**
   - Use trusted build agents
   - Isolate build environment
   - Scan build output for malware

### SBOM (Software Bill of Materials)

Generate SBOM for transparency:

```bash
# Using CycloneDX
dotnet tool install --global CycloneDX
dotnet CycloneDX MyApp.csproj -o sbom.xml
```

---

## Security Checklist

### Before Release

- [ ] All binaries signed with EV certificate
- [ ] Installer signed and verified
- [ ] Update mechanism signature verification implemented
- [ ] Dependencies scanned for vulnerabilities
- [ ] SBOM generated and published
- [ ] Code signing certificate backed up securely
- [ ] Build pipeline security reviewed
- [ ] Release notes include security fixes

### Continuous

- [ ] Monitor certificate expiration
- [ ] Review dependency updates weekly
- [ ] Scan signed binaries periodically
- [ ] Audit access to signing certificates
- [ ] Review build logs for anomalies

---

## Related Documentation

- [Authentication Patterns](./authentication-patterns.md) - Authentication implementation
- [Data Protection](./data-protection.md) - Encryption and secure storage
- [Vulnerability Prevention](./vulnerability-prevention.md) - Security best practices
- [Security Testing](./security-testing.md) - Testing methodologies

---

## References

- [Windows Authenticode](https://docs.microsoft.com/en-us/windows/win32/seccrypto/cryptography-tools#authenticode)
- [SignTool Documentation](https://docs.microsoft.com/en-us/windows/win32/seccrypto/signtool)
- [Code Signing Best Practices](https://docs.microsoft.com/en-us/windows-hardware/drivers/dashboard/code-signing-best-practices)
- [NIST Supply Chain Security](https://csrc.nist.gov/projects/supply-chain-risk-management)
