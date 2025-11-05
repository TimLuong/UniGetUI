# UI Component Examples

This directory contains example implementations of UI components following the UniGetUI Design System specifications.

## Directory Structure

```
examples/ui-components/
├── README.md                    (this file)
├── buttons/
│   ├── PrimaryButton.xaml      Example primary button implementation
│   ├── SecondaryButton.xaml    Example secondary button implementation
│   ├── IconButton.xaml         Example icon button implementation
│   └── ToggleButton.xaml       Example toggle button implementation
├── inputs/
│   ├── TextInput.xaml          Example text input implementation
│   ├── PasswordInput.xaml      Example password input implementation
│   ├── SearchInput.xaml        Example search input implementation
│   └── NumberInput.xaml        Example number input implementation
├── navigation/
│   ├── NavigationView.xaml     Example side navigation implementation
│   ├── TabView.xaml            Example tab navigation implementation
│   └── Breadcrumb.xaml         Example breadcrumb navigation implementation
├── dialogs/
│   ├── ConfirmDialog.xaml      Example confirmation dialog implementation
│   ├── FormDialog.xaml         Example form dialog implementation
│   └── AlertDialog.xaml        Example alert dialog implementation
└── feedback/
    ├── ProgressBar.xaml        Example progress bar implementation
    ├── Notification.xaml       Example notification/toast implementation
    └── SkeletonLoader.xaml     Example skeleton loader implementation
```

## Usage

These examples demonstrate how to implement components according to the design system specifications. Each example includes:

1. **Visual Implementation**: XAML markup showing structure and styling
2. **State Management**: How to handle different component states
3. **Accessibility**: Proper ARIA attributes and keyboard support
4. **Comments**: Inline documentation explaining design decisions

## Integration

To use these examples in your project:

1. Review the component specification in `/docs/ui-ux/component-specifications.md`
2. Check the design tokens in `/docs/ui-ux/design-tokens.json`
3. Adapt the example XAML to your specific needs
4. Ensure accessibility guidelines are followed
5. Test with keyboard navigation and screen readers

## Best Practices

- **Don't copy blindly**: Understand the component before using it
- **Follow specifications**: Stay aligned with design system guidelines
- **Test thoroughly**: Verify all states and interactions
- **Maintain accessibility**: Never compromise on accessibility features
- **Use design tokens**: Reference tokens instead of hardcoded values

## Contributing Examples

When adding new component examples:

1. Follow the existing structure and naming conventions
2. Include all component states (default, hover, focus, disabled, etc.)
3. Add accessibility attributes (ARIA labels, roles, keyboard support)
4. Document any deviations from standard patterns
5. Test with Windows Narrator or other screen readers

## Related Documentation

- [Design System](../../docs/ui-ux/design-system.md)
- [Component Specifications](../../docs/ui-ux/component-specifications.md)
- [Accessibility Guidelines](../../docs/ui-ux/accessibility-guidelines.md)
- [Design Tokens](../../docs/ui-ux/design-tokens.json)
