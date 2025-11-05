# Component Library Specifications

## Overview

This document provides detailed specifications for all UI components used in UniGetUI. Each component follows Windows 11 design principles and WinUI 3 standards, ensuring consistency, accessibility, and excellent user experience.

## Component Categories

- [Buttons](#buttons)
- [Input Controls](#input-controls)
- [Selection Controls](#selection-controls)
- [Navigation Components](#navigation-components)
- [Dialogs & Modals](#dialogs--modals)
- [Feedback Components](#feedback-components)
- [Data Display](#data-display)
- [Layout Components](#layout-components)

---

## Buttons

### Primary Button

**Purpose**: Main call-to-action, most important action on a page.

**Visual Specifications**
- Height: 32px (standard), 40px (large)
- Padding: 12px horizontal, 6px vertical
- Border radius: 4px
- Font: Body Strong (14px, Semibold)
- Min width: 120px

**States**
| State | Background | Text Color | Border |
|-------|-----------|------------|--------|
| Default | Primary | TextOnPrimary | None |
| Hover | PrimaryHover | TextOnPrimary | None |
| Pressed | PrimaryPressed | TextOnPrimary | None |
| Disabled | Surface | TextTertiary | 1px Divider |
| Focus | Primary | TextOnPrimary | 2px Accent (offset 2px) |

**Usage Guidelines**
- Limit to one per page section
- Use for primary actions (Save, Create, Submit)
- Always visible and accessible
- Avoid using for destructive actions

**Code Example**
```xaml
<Button Style="{StaticResource AccentButtonStyle}"
        Content="Save Changes"
        MinWidth="120"
        Height="32"/>
```

### Secondary Button

**Purpose**: Supporting actions, less prominent than primary.

**Visual Specifications**
- Height: 32px (standard), 40px (large)
- Padding: 12px horizontal, 6px vertical
- Border radius: 4px
- Font: Body (14px, Regular)
- Border: 1px Divider

**States**
| State | Background | Text Color | Border |
|-------|-----------|------------|--------|
| Default | Surface | TextPrimary | 1px Divider |
| Hover | SurfaceElevated | TextPrimary | 1px Primary |
| Pressed | Surface | TextPrimary | 1px Primary |
| Disabled | Surface | TextTertiary | 1px Divider |
| Focus | Surface | TextPrimary | 2px Accent (offset 2px) |

**Usage Guidelines**
- Use for secondary actions (Cancel, Back)
- Can have multiple per section
- Pair with primary button for choice scenarios

**Code Example**
```xaml
<Button Style="{StaticResource DefaultButtonStyle}"
        Content="Cancel"
        MinWidth="120"
        Height="32"/>
```

### Icon Button

**Purpose**: Actions represented by icons, space-efficient.

**Visual Specifications**
- Size: 32x32px (standard), 40x40px (large)
- Icon size: 16x16px (standard), 20x20px (large)
- Border radius: 4px
- Background: Transparent

**States**
| State | Background | Icon Color | Border |
|-------|-----------|-----------|--------|
| Default | Transparent | TextSecondary | None |
| Hover | Surface | TextPrimary | None |
| Pressed | SurfaceElevated | TextPrimary | None |
| Disabled | Transparent | TextTertiary | None |
| Focus | Surface | TextPrimary | 2px Accent (offset 2px) |

**Usage Guidelines**
- Always provide tooltip on hover
- Use recognizable icons only
- Consider text labels for critical actions
- Group related icon buttons with spacing

**Accessibility Requirements**
- ARIA label or tooltip required
- Keyboard accessible
- Minimum touch target: 44x44px (add transparent padding)

### Text Button (Link Button)

**Purpose**: Tertiary actions, navigation, inline links.

**Visual Specifications**
- Height: Auto (based on text)
- Padding: 4px horizontal
- No background or border
- Font: Body (14px, Regular)
- Text decoration: Underline on hover

**States**
| State | Background | Text Color | Decoration |
|-------|-----------|-----------|------------|
| Default | Transparent | Primary | None |
| Hover | Transparent | PrimaryHover | Underline |
| Pressed | Transparent | PrimaryPressed | Underline |
| Disabled | Transparent | TextTertiary | None |
| Focus | Transparent | Primary | 1px dotted (outline) |

**Usage Guidelines**
- Use for less important actions
- Inline with text when appropriate
- Navigation between views
- "Learn more" style links

### Toggle Button

**Purpose**: On/off states, show/hide actions.

**Visual Specifications**
- Same dimensions as secondary button
- Maintains state after click
- Visual indication of active state

**States**
| State | Background | Text Color | Border |
|-------|-----------|-----------|--------|
| Default (Off) | Surface | TextPrimary | 1px Divider |
| Active (On) | Primary (10% opacity) | Primary | 1px Primary |
| Hover (Off) | SurfaceElevated | TextPrimary | 1px Primary |
| Hover (On) | Primary (15% opacity) | Primary | 1px Primary |
| Disabled | Surface | TextTertiary | 1px Divider |

**Usage Guidelines**
- Use for binary choices that persist
- Provide clear visual feedback of state
- Common in toolbars and filters

---

## Input Controls

### Text Input (TextBox)

**Purpose**: Single-line text entry.

**Visual Specifications**
- Height: 32px (standard), 40px (large)
- Padding: 8px horizontal
- Border radius: 4px
- Font: Body (14px, Regular)
- Border: 1px Divider

**States**
| State | Background | Text Color | Border |
|-------|-----------|-----------|--------|
| Default | Surface | TextPrimary | 1px Divider |
| Hover | Surface | TextPrimary | 1px TextSecondary |
| Focus | Surface | TextPrimary | 2px Primary |
| Error | Surface | TextPrimary | 2px Error |
| Disabled | Surface | TextTertiary | 1px Divider |
| Read-only | Background | TextSecondary | None |

**Additional Elements**
- **Label**: Caption Strong (12px, Semibold) above input
- **Helper Text**: Caption (12px, Regular) below input
- **Error Message**: Caption (12px, Regular), Error color
- **Character Counter**: Caption, TextTertiary, right-aligned
- **Clear Button**: Icon button (×) when not empty

**Usage Guidelines**
- Always provide clear labels
- Use placeholder text for examples, not instructions
- Show error messages below input
- Validate on blur, not on every keystroke
- Support keyboard navigation (Tab, Enter)

**Accessibility Requirements**
- Associate label with input (for/id or aria-labelledby)
- Error messages in aria-live region
- Provide aria-invalid when error state
- Support screen reader announcements

**Code Example**
```xaml
<StackPanel Spacing="4">
    <TextBlock Text="Email Address" Style="{StaticResource CaptionStrongTextBlockStyle}"/>
    <TextBox PlaceholderText="example@domain.com"
             Height="32"
             x:Name="EmailInput"/>
    <TextBlock Text="We'll never share your email"
               Style="{StaticResource CaptionTextBlockStyle}"
               Foreground="{ThemeResource TextSecondaryBrush}"/>
</StackPanel>
```

### Multi-line Text Input (TextArea)

**Purpose**: Multi-line text entry, longer content.

**Visual Specifications**
- Min height: 80px
- Padding: 8px
- Border radius: 4px
- Font: Body (14px, Regular)
- Resizable: Vertical only
- Scrollbar: Auto-appear

**Additional Features**
- Character/word counter
- Resize handle (bottom-right)
- Scroll to bottom button (if content exceeds viewport)

**Usage Guidelines**
- Default to 3-4 lines visible
- Show character limit proactively
- Allow paste formatting option
- Support Ctrl+Enter for submit (if applicable)

### Number Input

**Purpose**: Numeric value entry with increment/decrement.

**Visual Specifications**
- Height: 32px
- Includes spinner buttons (up/down arrows)
- Right-aligned text
- Border radius: 4px

**Features**
- Step value (default: 1)
- Min/max constraints
- Decimal precision control
- Keyboard support (Arrow keys, Page Up/Down)

**Usage Guidelines**
- Use for numeric values with reasonable ranges
- Show unit of measurement (%, px, etc.)
- Disable spinner if min/max reached
- Validate input continuously

### Password Input

**Purpose**: Secure text entry for passwords.

**Visual Specifications**
- Same as Text Input
- Password reveal button (eye icon) on right
- Masked characters (•••)

**Features**
- Toggle visibility button
- Strength indicator (optional)
- Caps lock warning
- Paste prevention (optional, security-sensitive)

**Usage Guidelines**
- Don't show password by default
- Provide clear toggle for visibility
- Show caps lock indicator
- Consider passphrase support
- Follow platform password rules

### Search Input

**Purpose**: Search functionality with autocomplete.

**Visual Specifications**
- Height: 32px
- Search icon on left (16x16px)
- Clear button on right when not empty
- Border radius: 16px (full pill shape)

**Features**
- Instant search (debounced)
- Search suggestions dropdown
- Recent searches
- Clear search button
- Keyboard shortcuts (Ctrl+F, Esc to clear)

**Usage Guidelines**
- Provide instant feedback
- Show search results dynamically
- Highlight matching text
- Support keyboard navigation in results
- Clear search state appropriately

---

## Selection Controls

### Checkbox

**Purpose**: Multiple selection, binary choice.

**Visual Specifications**
- Size: 20x20px
- Border radius: 4px
- Border: 2px Divider
- Checkmark: 12x12px

**States**
| State | Background | Border | Checkmark |
|-------|-----------|--------|-----------|
| Unchecked | Transparent | 2px Divider | None |
| Checked | Primary | 2px Primary | TextOnPrimary |
| Indeterminate | Primary | 2px Primary | Dash (−) |
| Hover (Unchecked) | Surface | 2px TextSecondary | None |
| Hover (Checked) | PrimaryHover | 2px PrimaryHover | TextOnPrimary |
| Disabled (Unchecked) | Surface | 2px Divider | None |
| Disabled (Checked) | Surface | 2px Divider | TextTertiary |
| Focus | (ring) | 2px Accent (offset 2px) | (as current state) |

**Label Specifications**
- Font: Body (14px, Regular)
- Position: Right of checkbox, 8px spacing
- Clickable: Entire label area toggles checkbox
- Multi-line: Label wraps, checkbox aligns with first line

**Usage Guidelines**
- Use for multiple selections
- Group related checkboxes
- Provide "Select All" for long lists
- Support indeterminate state for nested selections
- Use consistent vertical spacing (8px)

**Accessibility Requirements**
- Label associated with checkbox
- Keyboard toggleable (Space)
- Announce state changes
- Visual focus indicator

### Radio Button

**Purpose**: Single selection from multiple options.

**Visual Specifications**
- Size: 20x20px
- Shape: Circle
- Border: 2px Divider
- Inner circle: 8x8px when selected

**States**
| State | Background | Border | Inner Circle |
|-------|-----------|--------|--------------|
| Unselected | Transparent | 2px Divider | None |
| Selected | Transparent | 2px Primary | Primary (8x8px) |
| Hover (Unselected) | Surface | 2px TextSecondary | None |
| Hover (Selected) | Surface | 2px Primary | Primary |
| Disabled (Unselected) | Surface | 2px Divider | None |
| Disabled (Selected) | Surface | 2px Divider | TextTertiary (8x8px) |
| Focus | (ring) | 2px Accent (offset 2px) | (as current state) |

**Usage Guidelines**
- Use for mutually exclusive options
- Minimum 2, maximum 7 options
- Default selection recommended
- Vertical layout for better scannability
- Group related options

**Accessibility Requirements**
- Single tab stop for entire group
- Arrow keys to navigate within group
- Space to select
- Announce selection changes

### Toggle Switch

**Purpose**: Immediate binary state change (on/off).

**Visual Specifications**
- Width: 44px, Height: 20px
- Track border radius: 10px (full pill)
- Thumb: 16x16px circle
- Transition: 200ms ease

**States - Off**
| State | Track | Thumb | Thumb Position |
|-------|-------|-------|----------------|
| Default | Divider (border 2px) | Surface | Left |
| Hover | TextSecondary (border) | Surface | Left |
| Disabled | Divider (border) | Surface (opacity 0.5) | Left |

**States - On**
| State | Track | Thumb | Thumb Position |
|-------|-------|-------|----------------|
| Default | Primary | TextOnPrimary | Right |
| Hover | PrimaryHover | TextOnPrimary | Right |
| Disabled | Primary (opacity 0.5) | TextOnPrimary (opacity 0.5) | Right |

**Label Specifications**
- Font: Body (14px, Regular)
- Position: Right of switch, 12px spacing
- Optional sublabel: Caption, TextSecondary

**Usage Guidelines**
- Use for immediate effect settings
- Provide clear on/off labels if ambiguous
- Don't require "Save" button
- Use for binary preferences
- Consider toggle button for UI state (toolbars)

**Accessibility Requirements**
- Role="switch"
- aria-checked state
- Keyboard toggleable (Space)
- Visual focus indicator

### Dropdown (ComboBox)

**Purpose**: Single selection from list of options.

**Visual Specifications**
- Height: 32px (collapsed)
- Padding: 8px horizontal
- Border radius: 4px
- Chevron icon: 12x12px (right side, 8px from edge)
- Border: 1px Divider

**Dropdown Menu**
- Max height: 320px (scrollable if more)
- Item height: 32px
- Item padding: 8px horizontal
- Border radius: 8px
- Shadow: Level 2
- Margin: 4px from trigger

**States - Trigger**
| State | Background | Text Color | Border |
|-------|-----------|-----------|--------|
| Default | Surface | TextPrimary | 1px Divider |
| Hover | Surface | TextPrimary | 1px TextSecondary |
| Open | Surface | TextPrimary | 2px Primary |
| Disabled | Surface | TextTertiary | 1px Divider |
| Focus | Surface | TextPrimary | 2px Primary |

**States - Items**
| State | Background | Text Color |
|-------|-----------|-----------|
| Default | Transparent | TextPrimary |
| Hover | SurfaceElevated | TextPrimary |
| Selected | Primary (10% opacity) | Primary |
| Disabled | Transparent | TextTertiary |

**Features**
- Type-ahead search
- Keyboard navigation (Arrow keys, Home, End)
- Clear selection option (if nullable)
- Placeholder text when empty
- Icon support (left of text)

**Usage Guidelines**
- Use for 4+ options (3 or less: radio buttons)
- Sort alphabetically or by usage frequency
- Provide search for 10+ items
- Group related items with dividers
- Show selected value clearly

**Accessibility Requirements**
- Role="combobox"
- aria-expanded state
- aria-activedescendant for highlighted item
- Keyboard navigation fully supported

### Multi-Select Dropdown

**Purpose**: Multiple selections from list.

**Visual Specifications**
- Same as Dropdown
- Selected items shown as chips in trigger
- Max display: 2 chips + count ("+3 more")

**Features**
- Checkboxes in dropdown items
- "Select All" / "Clear All" options
- Search/filter capability
- Chip badges for selections

**Usage Guidelines**
- Use when multiple selections allowed
- Consider checklist for always-visible options
- Provide clear way to view all selections
- Support removal via chip × button

---

## Navigation Components

### Navigation View (Side Navigation)

**Purpose**: Primary navigation for application sections.

**Visual Specifications**
- Width: 320px (expanded), 48px (collapsed)
- Background: Surface
- Border: 1px Divider (right edge)
- Item height: 40px
- Icon size: 20x20px
- Collapse animation: 200ms

**Navigation Item States**
| State | Background | Text Color | Icon Color | Border |
|-------|-----------|-----------|-----------|--------|
| Default | Transparent | TextPrimary | TextSecondary | None |
| Hover | SurfaceElevated | TextPrimary | TextPrimary | None |
| Selected | Primary (10% opacity) | Primary | Primary | 3px Primary (left) |
| Disabled | Transparent | TextTertiary | TextTertiary | None |
| Focus | SurfaceElevated | TextPrimary | TextPrimary | 2px Accent (inset) |

**Structure**
- Header section (optional)
- Primary navigation items
- Divider
- Secondary/utility items (bottom)
- Footer section (optional)
- Collapse toggle button

**Features**
- Hierarchical navigation (expandable groups)
- Search/filter items
- Badge support for notifications
- Keyboard navigation (Arrow keys, Tab)
- Responsive (auto-collapse on mobile)

**Usage Guidelines**
- Maximum 7-9 primary items
- Group related items
- Place most important items at top
- Settings and profile at bottom
- Indicate current page clearly

**Accessibility Requirements**
- Role="navigation"
- aria-label for navigation region
- aria-current="page" for active item
- Keyboard accessible
- Screen reader friendly

### Tab Navigation

**Purpose**: Switch between related content views.

**Visual Specifications**
- Tab height: 36px
- Tab padding: 12px horizontal
- Font: Body (14px, Regular)
- Active indicator: 2px Primary (bottom border)
- Min width: 100px (or auto-fit content)

**Tab States**
| State | Background | Text Color | Indicator |
|-------|-----------|-----------|-----------|
| Default | Transparent | TextSecondary | None |
| Hover | Surface | TextPrimary | None |
| Active | Transparent | Primary | 2px Primary (bottom) |
| Disabled | Transparent | TextTertiary | None |
| Focus | Surface | (as current) | 2px Accent (outline) |

**Variants**
- **Horizontal Tabs**: Default, below header
- **Vertical Tabs**: Left-aligned, for side-by-side content
- **Pill Tabs**: Rounded, filled background when active

**Usage Guidelines**
- Maximum 5-7 tabs
- Equal-width or auto-width options
- Scroll horizontally if overflow
- Icon support (above or left of text)
- Badge support for notifications

**Accessibility Requirements**
- Role="tablist" for container
- Role="tab" for each tab
- Role="tabpanel" for content
- aria-selected for active tab
- Arrow key navigation between tabs

### Breadcrumb Navigation

**Purpose**: Show hierarchical location, quick navigation.

**Visual Specifications**
- Height: 32px
- Font: Caption (12px, Regular)
- Separator: Chevron right (›) or slash (/)
- Separator spacing: 8px each side
- Link color: TextSecondary
- Active color: TextPrimary (not clickable)

**Breadcrumb Item States**
| State | Text Color | Decoration |
|-------|-----------|-----------|
| Default | TextSecondary | None |
| Hover | Primary | Underline |
| Active (current) | TextPrimary | None (not clickable) |
| Focus | Primary | 1px dotted outline |

**Features**
- Truncate middle items on narrow screens
- Show full path on hover/tooltip
- Icon support for home/root
- Ellipsis (…) for collapsed items

**Usage Guidelines**
- Maximum 4-5 levels visible
- Home icon for root
- Current page not clickable
- Consider dropdown for truncated items
- Responsive collapse strategy

### Command Bar (Toolbar)

**Purpose**: Context-specific actions and commands.

**Visual Specifications**
- Height: 48px
- Background: Surface
- Border: 1px Divider (bottom)
- Button spacing: 4px
- Icon button size: 32x32px

**Structure**
- Primary actions (left)
- Search/filter (center, if applicable)
- Secondary actions (right)
- Overflow menu (more options)

**Usage Guidelines**
- Maximum 5-7 visible actions
- Group related actions
- Most important actions on left
- Overflow menu for less common actions
- Icons with tooltips
- Disabled states when actions unavailable

---

## Dialogs & Modals

### Dialog (Modal)

**Purpose**: Focused task, important information, user decision.

**Visual Specifications**
- Max width: 600px (medium), 800px (large)
- Padding: 24px
- Border radius: 12px
- Background: Surface
- Shadow: Level 3
- Overlay: Background (60% opacity)

**Structure**
- **Header**: Title (Title, 20px Semibold), Close button (top-right)
- **Content**: Scrollable if necessary, max-height: 70vh
- **Footer**: Actions (right-aligned), Primary + Secondary buttons

**Dialog Types**

#### Confirmation Dialog
- Simple message
- Primary action (destructive or constructive)
- Secondary action (cancel)
- Optional checkbox ("Don't ask again")

#### Form Dialog
- Input fields
- Validation messages
- Submit and Cancel buttons
- Required field indicators

#### Alert Dialog
- Icon (error, warning, info, success)
- Title and message
- Single action button (OK)

**Usage Guidelines**
- Use sparingly, interrupts workflow
- Clear, concise title
- Explain consequences of actions
- Provide obvious way to dismiss
- Avoid stacking modals
- Trap focus within dialog
- Escape key to close (unless destructive)

**Accessibility Requirements**
- Role="dialog" or "alertdialog"
- aria-labelledby for title
- aria-describedby for content
- Focus trap (Tab cycles within dialog)
- Focus on first interactive element
- Return focus on close
- Escape to close

### Bottom Sheet (Mobile)

**Purpose**: Mobile-optimized dialog alternative.

**Visual Specifications**
- Width: 100% of viewport
- Border radius: 12px (top corners)
- Background: Surface
- Handle: 32px wide, 4px tall, centered (drag indicator)
- Slide-up animation: 300ms

**Features**
- Swipe down to dismiss
- Backdrop dismiss (tap outside)
- Expandable to full screen
- Supports scrolling content

**Usage Guidelines**
- Use on mobile instead of center dialogs
- Provide drag handle
- Support both tap and swipe close
- Animate smoothly

### Flyout (Popover)

**Purpose**: Contextual information, lightweight actions.

**Visual Specifications**
- Max width: 400px
- Padding: 12px
- Border radius: 8px
- Background: Surface
- Shadow: Level 2
- Arrow pointer: 8px (pointing to trigger)

**Usage Guidelines**
- Position near trigger element
- Auto-adjust position if near viewport edge
- Dismiss on outside click
- Close on selection (if action list)
- Lightweight, non-critical content

### Tooltip

**Purpose**: Supplementary information on hover/focus.

**Visual Specifications**
- Max width: 320px
- Padding: 8px 12px
- Border radius: 4px
- Background: Inverted from theme (dark in light mode)
- Text: TextOnPrimary, Caption (12px)
- Shadow: Level 1
- Arrow: 6px

**Behavior**
- Appear after 500ms hover
- Dismiss immediately on mouse leave
- Position: Above element (or adjust if no space)
- No interactive content

**Usage Guidelines**
- Brief, helpful text only
- Explain icon buttons
- Show keyboard shortcuts
- Provide context for truncated text
- Don't repeat visible label

---

## Feedback Components

### Progress Indicators

#### Linear Progress Bar

**Visual Specifications**
- Height: 4px (thin), 8px (standard)
- Border radius: 2px (thin), 4px (standard)
- Track: Divider
- Fill: Primary

**Types**
- Determinate: Shows specific percentage
- Indeterminate: Animated slide for unknown duration

**Usage Guidelines**
- Show for operations > 2 seconds
- Determinate when progress can be calculated
- Indeterminate when duration unknown
- Display percentage text if space allows

#### Circular Progress (Spinner)

**Visual Specifications**
- Size: 16px (small), 24px (medium), 32px (large)
- Stroke width: 2px (small), 3px (medium), 4px (large)
- Color: Primary
- Animation: 1s linear infinite rotation

**Usage Guidelines**
- Use for loading states
- Center in loading areas
- Provide loading message nearby
- Replace with content when loaded

#### Progress Ring (Determinate)

**Visual Specifications**
- Size: 48px (default), 64px (large)
- Stroke width: 4px
- Track: Divider
- Fill: Primary
- Percentage in center: Title (20px)

**Usage Guidelines**
- Show task completion percentage
- Use for file uploads, installations
- Display time remaining if available

### Notifications (Toast)

**Purpose**: Transient messages, system feedback.

**Visual Specifications**
- Width: 360px
- Min height: 64px
- Padding: 16px
- Border radius: 8px
- Background: Surface
- Shadow: Level 3
- Border: 1px (severity color)

**Notification Types**
| Type | Icon | Border Color | Duration |
|------|------|-------------|----------|
| Info | Info icon (i) | Info | 4s |
| Success | Checkmark | Success | 4s |
| Warning | Warning triangle | Warning | 6s |
| Error | Error X | Error | 8s (or manual) |

**Structure**
- Icon (left, 20x20px)
- Title (Body Strong)
- Message (Caption)
- Actions (optional, bottom)
- Close button (top-right)

**Behavior**
- Slide in from top-right (desktop)
- Slide up from bottom (mobile)
- Auto-dismiss after duration
- Stack multiple notifications
- Pause on hover (desktop)

**Usage Guidelines**
- Brief, actionable messages
- Use appropriate severity level
- Provide action buttons when relevant
- Don't stack too many (max 3 visible)
- Critical errors require manual dismiss

### Banner (Alert)

**Purpose**: Persistent system-wide messages.

**Visual Specifications**
- Width: 100% of container
- Min height: 48px
- Padding: 12px 16px
- Border radius: 4px (or sharp for full-width)
- Background: Severity color (10% opacity)
- Border: 1px severity color (left or full border)

**Banner Types**
- **Informational**: Blue background, info icon
- **Success**: Green background, checkmark icon
- **Warning**: Yellow background, warning icon
- **Error**: Red background, error icon

**Structure**
- Icon (left)
- Message (Body)
- Action button (optional, right)
- Close button (far right)

**Usage Guidelines**
- Persistent until dismissed
- Use for system-wide issues
- Place at top of page/section
- Provide resolution action
- Single banner at a time (per context)

### Skeleton Loader

**Purpose**: Content placeholder during loading.

**Visual Specifications**
- Shape matches content (rectangles, circles)
- Background: Divider
- Animation: Shimmer effect (light sweep) 1.5s infinite
- Border radius: Matches target content

**Usage Guidelines**
- Match layout of actual content
- Use for perceived performance
- Show immediately, no delay
- Replace with real content instantly
- Consider progressive loading

---

## Data Display

### Table (DataGrid)

**Purpose**: Structured data in rows and columns.

**Visual Specifications**
- Row height: 40px (standard), 48px (comfortable)
- Cell padding: 8px horizontal, 12px vertical
- Border: 1px Divider (horizontal between rows)
- Header background: Surface
- Header font: Body Strong (14px, Semibold)
- Body font: Body (14px, Regular)

**Header Features**
- Sortable columns (arrow indicator)
- Column resize handles
- Fixed header (scrollable body)
- Column menu (filter, hide)

**Row States**
| State | Background | Text Color | Border |
|-------|-----------|-----------|--------|
| Default | Surface | TextPrimary | 1px Divider (bottom) |
| Hover | SurfaceElevated | TextPrimary | 1px Divider |
| Selected | Primary (10% opacity) | Primary | 1px Primary (bottom) |
| Focus | SurfaceElevated | TextPrimary | 2px Accent (inset) |

**Features**
- Row selection (checkbox in first column)
- Multi-select support
- Pagination or infinite scroll
- Empty state message
- Loading state (skeleton)
- Bulk actions toolbar (when selected)

**Usage Guidelines**
- Use for comparing data
- Keep columns to minimum (5-8 visible)
- Right-align numeric data
- Truncate long text with tooltip
- Provide search/filter
- Export option for large datasets

### List

**Purpose**: Vertical list of items.

**Visual Specifications**
- Item height: 48px (single line), 64px (two lines)
- Item padding: 16px horizontal, 12px vertical
- Divider: 1px Divider (between items)
- Icon size: 20x20px (left side)
- Action buttons: Right side

**List Item Components**
- **Leading**: Icon or avatar (left)
- **Content**: Title (Body) and optional subtitle (Caption)
- **Trailing**: Secondary text, icon, or action button (right)

**Item States**
| State | Background | Text Color |
|-------|-----------|-----------|
| Default | Surface | TextPrimary |
| Hover | SurfaceElevated | TextPrimary |
| Selected | Primary (10% opacity) | Primary |
| Disabled | Surface | TextTertiary |
| Focus | SurfaceElevated | TextPrimary (2px Accent outline) |

**List Types**
- **Simple List**: Text only
- **Icon List**: Icon + text
- **Avatar List**: Avatar + text + secondary text
- **Multi-line List**: Title + description
- **Interactive List**: Clickable items, navigation

**Usage Guidelines**
- Use for homogeneous data
- Consistent item height within list
- Provide actions on hover or trailing
- Support keyboard navigation
- Empty state for no items

### Card

**Purpose**: Contained content unit, related information.

**Visual Specifications**
- Padding: 16px (standard), 24px (comfortable)
- Border radius: 8px
- Background: Surface
- Border: 1px Divider (optional)
- Shadow: Level 1 (default), Level 2 (elevated)

**Card Structure**
- **Header**: Title (Subtitle, 16px Semibold), optional actions
- **Media**: Image or icon (optional, top or left)
- **Content**: Body text, description
- **Footer**: Actions, metadata (optional)

**Card States**
| State | Background | Border | Shadow |
|-------|-----------|--------|--------|
| Default | Surface | 1px Divider | Level 1 |
| Hover (interactive) | Surface | 1px Primary | Level 2 |
| Focus | Surface | 2px Primary | Level 1 |
| Selected | Primary (5% opacity) | 2px Primary | Level 1 |

**Card Types**
- **Static Card**: Display only, no interaction
- **Clickable Card**: Entire card is clickable
- **Actionable Card**: Contains interactive elements

**Usage Guidelines**
- Group related information
- Use consistent card sizes in a grid
- Provide clear visual hierarchy
- Limit actions (2-3 max)
- Consider image aspect ratio

### Badge

**Purpose**: Small status indicator, count display.

**Visual Specifications**
- Height: 20px (standard), 16px (small)
- Padding: 4px 8px
- Border radius: 10px (pill)
- Font: Caption Strong (12px, Semibold)

**Badge Types**
| Type | Background | Text Color | Usage |
|------|-----------|-----------|-------|
| Default | Surface | TextPrimary | Neutral info |
| Primary | Primary | TextOnPrimary | Primary status |
| Success | Success | TextOnPrimary | Positive status |
| Warning | Warning | TextPrimary | Caution |
| Error | Error | TextOnPrimary | Negative status |
| Info | Info | TextOnPrimary | Information |

**Badge Positions**
- Inline: Next to text
- Overlap: Top-right of icon/avatar (notification count)
- Standalone: Status indicator

**Usage Guidelines**
- Keep text short (1-2 words or number)
- Use color meaningfully
- Consider icon + badge combination
- Notification count: "99+" for 100+

### Avatar

**Purpose**: User representation, profile image.

**Visual Specifications**
- Size: 24px (xs), 32px (sm), 40px (md), 48px (lg), 64px (xl)
- Shape: Circle (person), Square with radius (groups, items)
- Border radius: Full circle or 8px
- Border: 2px Surface (on colored backgrounds)

**Avatar Types**
- **Image**: User photo
- **Initials**: 1-2 letters, colored background
- **Icon**: Generic person icon
- **Group**: Overlapping avatars

**Initials Background Colors**
- Use consistent color per user (hash username)
- Sufficient contrast for white text
- Avoid very bright or very dark colors

**Usage Guidelines**
- Prefer image when available
- Fallback to initials, then icon
- Show online status (badge overlay)
- Support badge for notifications
- Alt text with user name

---

## Layout Components

### Container

**Purpose**: Main content wrapper with max-width.

**Visual Specifications**
- Max width: 1440px
- Padding: 24px (desktop), 16px (mobile)
- Margin: 0 auto (centered)

### Stack (Vertical)

**Purpose**: Vertical layout with consistent spacing.

**Visual Specifications**
- Direction: Column
- Spacing: 8px (sm), 16px (md), 24px (lg)
- Alignment: Start, center, end, stretch

### Grid

**Purpose**: Multi-column responsive layout.

**Visual Specifications**
- Columns: 12-column system
- Gutter: 16px (desktop), 8px (mobile)
- Auto-fit columns based on content width
- Responsive breakpoints

**Usage Guidelines**
- Use for equal-width cards
- Auto-layout for unknown item count
- Explicit columns for fixed layouts

### Divider

**Purpose**: Visual separation of content.

**Visual Specifications**
- Horizontal: Width 100%, Height 1px
- Vertical: Width 1px, Height 100%
- Color: Divider
- Margin: 16px vertical (horizontal), 16px horizontal (vertical)

**Types**
- **Subtle**: 1px Divider color
- **Strong**: 2px, TextSecondary
- **With Label**: Text in center, lines on sides

### Spacer

**Purpose**: Flexible space between elements.

**Visual Specifications**
- Height/Width: Variable
- Common sizes: 8px, 16px, 24px, 32px
- Use for fine-tuned spacing

---

## Component Usage Best Practices

### Consistency
- Use same components for same functions across application
- Follow specifications exactly
- Maintain visual rhythm

### Accessibility
- Ensure keyboard navigation works
- Provide proper ARIA labels
- Test with screen readers
- Meet WCAG 2.1 AA standards

### Performance
- Lazy load components when possible
- Virtualize long lists
- Optimize animations
- Minimize re-renders

### Testing
- Test all interactive states
- Validate on different screen sizes
- Check color contrast
- Test with assistive technologies

## Version Control

**Version**: 1.0.0  
**Last Updated**: November 2025  
**Maintained By**: UniGetUI Design Team

## Related Documentation

- [Design System](./design-system.md)
- [Accessibility Guidelines](./accessibility-guidelines.md)
- [Design Tokens](./design-tokens.json)
