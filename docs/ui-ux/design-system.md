# UniGetUI Design System

## Overview

The UniGetUI Design System is a comprehensive guide for creating consistent, accessible, and visually appealing user interfaces for Windows applications. This design system aligns with Windows 11 design language principles, WinUI 3, and Fluent Design System standards.

## Design Philosophy

### Core Principles

1. **Consistency**: Maintain visual and functional consistency across all UI elements
2. **Clarity**: Ensure interfaces are clear, intuitive, and easy to understand
3. **Efficiency**: Optimize workflows and reduce cognitive load for users
4. **Accessibility**: Design for all users, including those with disabilities
5. **Flexibility**: Support customization while maintaining coherence

### Windows 11 Design Language Compliance

UniGetUI follows the Windows 11 design language, incorporating:

- **Rounded corners**: Modern, soft edges (4px, 8px radius)
- **Mica material**: Translucent backgrounds that reflect desktop wallpaper
- **Acrylic effects**: Layered transparency for depth
- **Fluent animations**: Smooth, purposeful motion
- **Modern typography**: Segoe UI Variable font family
- **Adaptive UI**: Responsive to system themes and user preferences

## Design Tokens

Design tokens are the atomic design decisions that define the visual characteristics of the design system. See [design-tokens.json](./design-tokens.json) for complete specifications.

### Color System

#### Light Theme

**Primary Colors**
- `Primary`: #0067C0 - Main brand color for primary actions
- `PrimaryHover`: #005A9E - Hover state for primary elements
- `PrimaryPressed`: #004D84 - Pressed state for primary elements

**Accent Colors**
- `Accent`: #0078D4 - System accent color (Windows theme-aware)
- `AccentLight`: #4A9EE5 - Light variant
- `AccentDark`: #005A9E - Dark variant

**Neutral Colors**
- `Background`: #F9F9F9 - Main background
- `Surface`: #FFFFFF - Card and surface background
- `SurfaceElevated`: #FFFFFF - Elevated surface (with shadow)
- `Divider`: #E5E5E5 - Divider lines and borders

**Text Colors**
- `TextPrimary`: #1F1F1F - Primary text
- `TextSecondary`: #616161 - Secondary text
- `TextTertiary`: #8A8A8A - Tertiary text and disabled state
- `TextOnPrimary`: #FFFFFF - Text on primary colored backgrounds

**Semantic Colors**
- `Success`: #107C10 - Success states, confirmations
- `Warning`: #FCE100 - Warning states, caution
- `Error`: #D13438 - Error states, destructive actions
- `Info`: #0078D4 - Informational messages

#### Dark Theme

**Primary Colors**
- `Primary`: #60CDFF - Main brand color for primary actions
- `PrimaryHover`: #88D9FF - Hover state
- `PrimaryPressed`: #3CB4E5 - Pressed state

**Neutral Colors**
- `Background`: #202020 - Main background
- `Surface`: #2C2C2C - Card and surface background
- `SurfaceElevated`: #333333 - Elevated surface
- `Divider`: #3F3F3F - Divider lines and borders

**Text Colors**
- `TextPrimary`: #FFFFFF - Primary text
- `TextSecondary`: #C5C5C5 - Secondary text
- `TextTertiary`: #8A8A8A - Tertiary text and disabled state
- `TextOnPrimary`: #000000 - Text on primary colored backgrounds

**Semantic Colors**
- `Success`: #6CCB5F - Success states
- `Warning`: #FFF100 - Warning states
- `Error`: #FF4343 - Error states
- `Info`: #60CDFF - Informational messages

### Typography

**Font Family**: Segoe UI Variable, Segoe UI, sans-serif

#### Type Scale

| Style | Size | Weight | Line Height | Usage |
|-------|------|--------|-------------|-------|
| Display | 28px | 600 (Semibold) | 36px | Page titles, major headings |
| Title Large | 24px | 600 (Semibold) | 32px | Section headers |
| Title | 20px | 600 (Semibold) | 28px | Card headers, dialog titles |
| Subtitle | 16px | 600 (Semibold) | 24px | Group headers, subheadings |
| Body Strong | 14px | 600 (Semibold) | 20px | Emphasized body text |
| Body | 14px | 400 (Regular) | 20px | Default body text |
| Caption Strong | 12px | 600 (Semibold) | 16px | Small emphasized text |
| Caption | 12px | 400 (Regular) | 16px | Small text, labels, metadata |

### Spacing System

Based on 4px grid system for consistent spatial relationships:

| Token | Value | Usage |
|-------|-------|-------|
| `space-xs` | 4px | Minimal spacing, tight elements |
| `space-sm` | 8px | Compact spacing, related items |
| `space-md` | 12px | Default spacing between elements |
| `space-lg` | 16px | Section spacing, grouped content |
| `space-xl` | 24px | Major section spacing |
| `space-2xl` | 32px | Page-level spacing |
| `space-3xl` | 48px | Maximum spacing, page boundaries |

### Elevation & Shadows

Shadows create depth and hierarchy in the interface.

| Level | Shadow | Usage |
|-------|--------|-------|
| Level 0 | None | Flat elements, no elevation |
| Level 1 | 0px 1px 3px rgba(0,0,0,0.08) | Subtle elevation, cards |
| Level 2 | 0px 2px 6px rgba(0,0,0,0.12) | Moderate elevation, dropdowns |
| Level 3 | 0px 4px 12px rgba(0,0,0,0.16) | High elevation, modals |
| Level 4 | 0px 8px 24px rgba(0,0,0,0.20) | Maximum elevation, popovers |

### Border Radius

Consistent corner rounding for modern appearance:

| Token | Value | Usage |
|-------|-------|-------|
| `radius-none` | 0px | Sharp corners (rare) |
| `radius-sm` | 4px | Buttons, inputs, small elements |
| `radius-md` | 8px | Cards, panels, medium elements |
| `radius-lg` | 12px | Large containers, dialogs |
| `radius-full` | 999px | Circular elements, pills |

### Icon System

**Icon Library**: Fluent UI System Icons (Microsoft's official icon set)

**Icon Sizes**
- Small: 16x16px - Inline icons, list items
- Medium: 20x20px - Default size for most UI elements
- Large: 24x24px - Prominent actions, headers
- Extra Large: 32x32px - Feature highlights, empty states

**Icon Guidelines**
- Use consistent stroke width (1.5px for 20x20)
- Maintain optical balance
- Ensure 1:1 aspect ratio
- Support both filled and regular variants
- Provide alternative text for accessibility

## Layout Patterns

### Grid System

**12-Column Responsive Grid**
- Gutter: 16px (adjusts to 8px on mobile)
- Margin: 24px (adjusts to 16px on mobile)
- Max width: 1440px (centered on larger screens)

### Breakpoints

| Breakpoint | Width | Target Device |
|------------|-------|---------------|
| Mobile | < 768px | Phones |
| Tablet | 768px - 1023px | Tablets, small laptops |
| Desktop | 1024px - 1439px | Standard laptops, desktops |
| Large | ≥ 1440px | Large desktops, external monitors |

### Navigation Structures

#### Primary Navigation

**NavigationView (WinUI 3)**
- Left navigation pane for main sections
- Collapsible for space efficiency
- Support for hierarchical navigation
- Adaptive to screen size (auto-collapse on mobile)

**Tab Navigation**
- Horizontal tabs for related content areas
- Maximum 5-7 tabs for optimal usability
- Clear visual indication of active tab

#### Secondary Navigation

**Breadcrumbs**
- Show current location in hierarchy
- Allow quick navigation to parent pages
- Truncate intelligently on narrow screens

**Command Bar**
- Context-specific actions at the top
- Primary actions on the left
- Secondary actions in overflow menu

## Responsive Design Principles

### Mobile-First Approach

1. Design for smallest screen first
2. Progressively enhance for larger screens
3. Ensure touch-friendly targets (minimum 44x44px)
4. Optimize for one-handed use on mobile

### Adaptive Layouts

- Stack vertically on mobile, grid on desktop
- Show/hide elements based on available space
- Use collapsible sections for complex content
- Prioritize essential actions on small screens

### Content Reflow

- Text wraps and reflows naturally
- Images scale proportionally
- Tables transform to stacked lists on mobile
- Forms adjust to single-column layout

## Dark Mode & Theme Switching

### Implementation Approach

**System Theme Integration**
- Automatically detect Windows theme preference
- Listen to system theme changes
- Provide manual override option in settings

**Theme Toggle**
- Global setting in application preferences
- Instant theme switching without restart
- Preserve user preference across sessions

### Design Considerations

**Contrast Requirements**
- Maintain 4.5:1 contrast ratio for text (WCAG AA)
- Test all color combinations in both themes
- Adjust shadows for visibility in dark mode
- Use borders when shadows aren't visible

**Color Adaptations**
- Invert primary color brightness
- Reduce saturation in dark mode
- Use warmer grays for better eye comfort
- Ensure semantic colors remain distinguishable

## Animation & Transitions

### Motion Principles

1. **Purposeful**: Animations should serve a function
2. **Natural**: Follow physics-based motion curves
3. **Subtle**: Don't distract from content
4. **Responsive**: Provide immediate feedback
5. **Respectful**: Honor system animation preferences

### Standard Transitions

| Interaction | Duration | Easing | Usage |
|-------------|----------|--------|-------|
| Hover | 100ms | EaseOut | Button hover states |
| Click/Tap | 150ms | EaseInOut | Button press feedback |
| Panel Open | 300ms | EaseOutCubic | Sidebars, drawers |
| Panel Close | 250ms | EaseInCubic | Sidebars, drawers |
| Fade In | 200ms | EaseIn | Content appearing |
| Fade Out | 150ms | EaseOut | Content disappearing |
| Slide In | 300ms | EaseOutQuint | Modals, notifications |
| Scale | 200ms | EaseOutBack | Pop-ups, tooltips |

### Reduced Motion

**Accessibility Consideration**
- Detect `prefers-reduced-motion` system setting
- Replace animations with instant transitions
- Maintain functionality without motion
- Provide toggle in accessibility settings

### Animation Guidelines

**Do's**
- Use entrance animations for new content
- Provide loading animations for async operations
- Animate state changes for clarity
- Keep durations under 500ms for most interactions

**Don'ts**
- Don't animate everything
- Avoid complex animation chains
- Don't use animation to hide performance issues
- Don't force users to wait for animations

## Best Practices

### Performance

- Optimize asset loading and caching
- Use vector graphics (SVG) for icons
- Implement lazy loading for images
- Minimize layout thrashing
- Test on lower-end hardware

### Internationalization

- Design for variable text length
- Support RTL (right-to-left) languages
- Use unicode-safe fonts
- Avoid text in images
- Test with different language strings

### Touch & Input

- Minimum touch target: 44x44px
- Adequate spacing between interactive elements
- Support keyboard navigation
- Provide visible focus indicators
- Enable mouse, touch, and pen input

### State Management

- Provide clear visual feedback for all states
- Show loading states for async operations
- Display error states with recovery options
- Maintain context during transitions
- Preserve scroll position when appropriate

## Resources

### Design Tools

- **Figma**: Windows 11 Design Kit
- **Adobe XD**: Fluent Design System Kit
- **Sketch**: Microsoft UI Kit

### Documentation

- [Windows 11 Design Principles](https://learn.microsoft.com/en-us/windows/apps/design/)
- [WinUI 3 Gallery](https://apps.microsoft.com/store/detail/winui-3-gallery/9P3JFPWWDZRC)
- [Fluent Design System](https://fluent2.microsoft.design/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

### Code Libraries

- **WinUI 3**: Official Windows UI library
- **Windows Community Toolkit**: Extended controls
- **Fluent UI System Icons**: Microsoft icon library

## Maintenance & Updates

This design system is a living document that evolves with:
- Windows platform updates
- User feedback and research
- Accessibility standards changes
- Technology advancements

**Version**: 1.0.0  
**Last Updated**: November 2025  
**Maintained By**: UniGetUI Design Team
