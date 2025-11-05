namespace UniGetUI.Examples.Testing;

/// <summary>
/// Example calculator class for demonstrating unit testing
/// </summary>
public class Calculator
{
    public int Add(int a, int b) => a + b;
    
    public int Subtract(int a, int b) => a - b;
    
    public int Multiply(int a, int b) => a * b;
    
    public double Divide(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException("Cannot divide by zero");
        
        return (double)a / b;
    }
    
    public int Factorial(int n)
    {
        if (n < 0)
            throw new ArgumentException("Cannot calculate factorial of negative number");
        
        if (n == 0 || n == 1)
            return 1;
        
        int result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }
    
    public bool IsPrime(int number)
    {
        if (number < 2)
            return false;
        
        if (number == 2)
            return true;
        
        if (number % 2 == 0)
            return false;
        
        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            if (number % i == 0)
                return false;
        }
        
        return true;
    }
}

/// <summary>
/// Basic unit tests demonstrating Facts and Theories
/// </summary>
public class BasicUnitTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(5, 3);
        
        // Assert
        Assert.Equal(8, result);
    }
    
    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(5, 3, 8)]
    [InlineData(-2, 2, 0)]
    [InlineData(0, 0, 0)]
    [InlineData(-5, -3, -8)]
    [InlineData(100, 200, 300)]
    public void Add_VariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Subtract_TwoNumbers_ReturnsCorrectDifference()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Subtract(10, 3);
        
        // Assert
        Assert.Equal(7, result);
    }
    
    [Theory]
    [InlineData(10, 5, 2)]
    [InlineData(8, 2, 4)]
    [InlineData(0, 0, 0)]
    [InlineData(-10, 5, -2)]
    public void Divide_ValidInputs_ReturnsCorrectQuotient(int a, int b, double expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Divide(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act & Assert
        var exception = Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
        Assert.Equal("Cannot divide by zero", exception.Message);
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(3, 6)]
    [InlineData(4, 24)]
    public void Factorial_ValidInputs_ReturnsCorrectResult(int n, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Factorial(n);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Factorial_NegativeNumber_ThrowsArgumentException()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => calculator.Factorial(-5));
    }
    
    [Theory]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(7, true)]
    [InlineData(11, true)]
    [InlineData(4, false)]
    [InlineData(6, false)]
    [InlineData(9, false)]
    [InlineData(1, false)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void IsPrime_VariousNumbers_ReturnsCorrectResult(int number, bool expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.IsPrime(number);
        
        // Assert
        Assert.Equal(expected, result);
    }
}

/// <summary>
/// Example string utility class for testing
/// </summary>
public class StringUtilities
{
    public string Reverse(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        char[] chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
    
    public bool IsPalindrome(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;
        
        string normalized = new string(input.Where(char.IsLetterOrDigit).ToArray()).ToLower();
        return normalized == Reverse(normalized);
    }
    
    public int CountWords(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;
        
        return input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

/// <summary>
/// Tests for string utilities
/// </summary>
public class StringUtilityTests
{
    [Theory]
    [InlineData("hello", "olleh")]
    [InlineData("world", "dlrow")]
    [InlineData("a", "a")]
    [InlineData("", "")]
    [InlineData("racecar", "racecar")]
    public void Reverse_VariousStrings_ReturnsReversedString(string input, string expected)
    {
        // Arrange
        var utilities = new StringUtilities();
        
        // Act
        var result = utilities.Reverse(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Reverse_NullString_ReturnsNull()
    {
        // Arrange
        var utilities = new StringUtilities();
        
        // Act
        var result = utilities.Reverse(null);
        
        // Assert
        Assert.Null(result);
    }
    
    [Theory]
    [InlineData("racecar", true)]
    [InlineData("A man a plan a canal Panama", true)]
    [InlineData("hello", false)]
    [InlineData("", true)]
    [InlineData("a", true)]
    [InlineData("Madam", true)]
    public void IsPalindrome_VariousStrings_ReturnsCorrectResult(string input, bool expected)
    {
        // Arrange
        var utilities = new StringUtilities();
        
        // Act
        var result = utilities.IsPalindrome(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("hello world", 2)]
    [InlineData("one", 1)]
    [InlineData("one two three", 3)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("word1\nword2\tword3", 3)]
    public void CountWords_VariousInputs_ReturnsCorrectCount(string input, int expected)
    {
        // Arrange
        var utilities = new StringUtilities();
        
        // Act
        var result = utilities.CountWords(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
