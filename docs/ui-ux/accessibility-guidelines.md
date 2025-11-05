# Accessibility Guidelines

## Overview

Accessibility ensures that UniGetUI can be used by everyone, including people with disabilities. This document provides comprehensive guidelines to achieve WCAG 2.1 AA compliance and create an inclusive user experience.

## Core Principles (POUR)

### 1. Perceivable
Information and user interface components must be presentable to users in ways they can perceive.

### 2. Operable
User interface components and navigation must be operable by all users.

### 3. Understandable
Information and the operation of the user interface must be understandable.

### 4. Robust
Content must be robust enough to be interpreted by a wide variety of user agents, including assistive technologies.

---

## WCAG 2.1 AA Compliance

### Level A (Minimum)
- Must be implemented for all new features
- Critical for basic accessibility

### Level AA (Target)
- Target compliance level for UniGetUI
- Covers most accessibility needs
- Industry standard for government and enterprise

### Level AAA (Aspirational)
- Implement where feasible
- Not required for all content
- Enhanced accessibility for complex interactions

---

## Visual Design Accessibility

### Color Contrast

#### Text Contrast Requirements (WCAG 1.4.3)

**Normal Text** (< 18pt or < 14pt bold)
- Minimum ratio: **4.5:1** against background
- Enhanced ratio: **7:1** (AAA)

**Large Text** (≥ 18pt or ≥ 14pt bold)
- Minimum ratio: **3:1** against background
- Enhanced ratio: **4.5:1** (AAA)

**Graphical Objects & UI Components** (WCAG 1.4.11)
- Minimum ratio: **3:1** against adjacent colors
- Applies to icons, borders, focus indicators

#### Color Contrast Testing

**Light Theme Examples**
```
✅ PASS: #1F1F1F text on #F9F9F9 background (16.1:1)
✅ PASS: #616161 text on #FFFFFF background (5.7:1)
✅ PASS: #0067C0 (Primary) on #FFFFFF background (4.6:1)
❌ FAIL: #8A8A8A text on #F9F9F9 background (3.1:1)
```

**Dark Theme Examples**
```
✅ PASS: #FFFFFF text on #202020 background (17.5:1)
✅ PASS: #C5C5C5 text on #2C2C2C background (8.1:1)
✅ PASS: #60CDFF (Primary) on #202020 background (8.8:1)
❌ FAIL: #8A8A8A text on #333333 background (2.8:1)
```

**Testing Tools**
- WebAIM Contrast Checker
- Chrome DevTools Accessibility Panel
- Accessibility Insights for Windows
- Colour Contrast Analyser (CCA)

#### Color Usage Guidelines

**Don't Rely on Color Alone** (WCAG 1.4.1)
- Use icons + color for status
- Use text labels + color for categories
- Use patterns + color for charts
- Provide text alternatives

**Examples**
```
❌ Bad: Red/green to indicate success/error
✅ Good: Green checkmark icon + "Success" text
         Red X icon + "Error" text

❌ Bad: Color-coded chart with no legend
✅ Good: Color + pattern + labels on chart
```

### Typography Accessibility

#### Font Size

**Minimum Sizes**
- Body text: 14px (0.875rem)
- Caption text: 12px (0.75rem) - use sparingly
- Avoid text smaller than 12px

**Resizable Text** (WCAG 1.4.4)
- Support zoom up to 200%
- Text must remain readable
- No horizontal scrolling at 200% zoom
- Use relative units (rem, em) not pixels

#### Font Weight & Style

**Readability**
- Minimum weight: 400 (Regular)
- Avoid thin fonts (< 400) for body text
- Use bold (600) for emphasis, not italic
- Italic acceptable for captions, quotes

#### Line Height & Spacing

**Line Height** (WCAG 1.4.12)
- Minimum: 1.5 × font size for body text
- Headings: 1.2 × font size minimum

**Paragraph Spacing**
- At least 2 × font size between paragraphs

**Letter Spacing**
- At least 0.12 × font size

**Word Spacing**
- At least 0.16 × font size

**Implementation**
```css
body {
  font-size: 14px;
  line-height: 21px; /* 1.5 × 14px */
  letter-spacing: 0.02em; /* ~0.12 × font size */
  word-spacing: 0.03em; /* ~0.16 × font size */
}
```

### Visual Indicators

#### Focus Indicators (WCAG 2.4.7)

**Requirements**
- Visible focus indicator on all interactive elements
- Minimum 2px thick border or outline
- Contrast ratio: 3:1 against background
- Clear visual distinction from non-focused state

**Implementation**
```css
:focus-visible {
  outline: 2px solid var(--accent-color);
  outline-offset: 2px;
}
```

**Interactive Elements**
- Buttons
- Links
- Form inputs
- Checkboxes/radio buttons
- Dropdowns
- Custom controls

#### Focus Order (WCAG 2.4.3)

**Logical Tab Order**
- Follow visual layout (left-to-right, top-to-bottom)
- Group related elements
- Skip links for main content
- Modal traps focus until dismissed

**Don't Use**
- Positive tabindex values (1, 2, 3...)
- Use tabindex="0" for custom interactive elements
- Use tabindex="-1" for programmatically focusable elements

### Motion & Animation

#### Reduced Motion (WCAG 2.3.3)

**System Setting Detection**
```css
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

**Accessibility Setting**
- Provide in-app toggle for reduced motion
- Respect system preference by default
- Essential motion only (loading indicators)

#### Animation Guidelines

**Avoid**
- Flashing content (more than 3 times per second)
- Parallax scrolling effects
- Automatic carousels that can't be paused
- Background videos that autoplay

**Safe Animations**
- Fade in/out
- Slide transitions (slow, controlled)
- Loading spinners
- Progress indicators

#### Pause, Stop, Hide (WCAG 2.2.2)

**Moving Content**
- Provide pause button
- Auto-pause after 5 seconds
- User control to restart

**Auto-updating Content**
- Provide pause/stop mechanism
- Manual refresh option
- Configurable update frequency

---

## Keyboard Accessibility

### Keyboard Navigation (WCAG 2.1.1)

#### All Functionality Available

**Requirements**
- Every interactive element accessible via keyboard
- No mouse-only functionality
- No keyboard traps (except modals)

**Standard Keys**
- **Tab**: Move forward through interactive elements
- **Shift+Tab**: Move backward
- **Enter**: Activate button/link
- **Space**: Activate button, toggle checkbox
- **Arrow Keys**: Navigate within component (list, tabs, menu)
- **Escape**: Close modal, cancel operation
- **Home/End**: Navigate to first/last item

#### Custom Component Keyboard Support

**Dropdown/Combobox**
- Arrow Up/Down: Navigate options
- Home/End: First/last option
- Enter/Space: Select option
- Escape: Close without selecting
- Type-ahead: Jump to matching option

**Tabs**
- Arrow Left/Right: Navigate tabs
- Home/End: First/last tab
- Tab: Move to tab panel content

**Modal Dialog**
- Tab: Cycle through interactive elements within modal
- Shift+Tab: Reverse cycle
- Escape: Close modal (if non-critical)

**Data Table**
- Arrow Keys: Navigate cells
- Home/End: First/last cell in row
- Ctrl+Home/End: First/last cell in table
- Space: Select row (if selectable)

### Skip Links (WCAG 2.4.1)

**Implementation**
```html
<a href="#main-content" class="skip-link">Skip to main content</a>
```

**Behavior**
- First focusable element on page
- Hidden until focused
- Jumps to main content area

**Additional Skip Links**
- Skip to navigation
- Skip to search
- Skip to footer

### Keyboard Shortcuts

**Guidelines**
- Single-key shortcuts only when in specific context
- Provide way to turn off or remap shortcuts
- Document all shortcuts
- Don't override browser shortcuts

**Common Shortcuts**
- Ctrl+F: Search
- Ctrl+S: Save
- Ctrl+Z: Undo
- Ctrl+N: New item
- Delete: Remove item
- F2: Rename/edit

**Conflict Avoidance**
- Don't use single letter keys globally
- Use Ctrl/Alt/Shift combinations
- Test with screen reader shortcuts

---

## Screen Reader Accessibility

### Semantic HTML (WCAG 1.3.1)

**Use Appropriate Elements**
```html
✅ Good:
<button>Click me</button>
<nav>...</nav>
<main>...</main>
<article>...</article>
<h1>Page Title</h1>

❌ Bad:
<div onclick="...">Click me</div>
<div class="nav">...</div>
<div class="content">...</div>
<div class="title">Page Title</div>
```

### ARIA (Accessible Rich Internet Applications)

#### ARIA Principles

**First Rule of ARIA**
- Use native HTML elements when possible
- ARIA only when HTML doesn't provide semantics

**Second Rule**
- Don't change native semantics

**Third Rule**
- All interactive ARIA controls must be keyboard accessible

#### Essential ARIA Attributes

**Landmarks**
```html
<header role="banner">
<nav role="navigation" aria-label="Main navigation">
<main role="main" aria-labelledby="page-title">
<aside role="complementary" aria-label="Related content">
<footer role="contentinfo">
```

**Labels**
```html
<!-- Label association -->
<label for="email">Email</label>
<input id="email" type="email">

<!-- ARIA label -->
<button aria-label="Close dialog">×</button>

<!-- ARIA labelled by -->
<div id="dialog-title">Confirm Action</div>
<div role="dialog" aria-labelledby="dialog-title">

<!-- ARIA described by -->
<input id="password" aria-describedby="password-rules">
<div id="password-rules">Must be at least 8 characters</div>
```

**States & Properties**
```html
<!-- Expanded/Collapsed -->
<button aria-expanded="false" aria-controls="menu">
  Menu
</button>
<div id="menu" hidden>...</div>

<!-- Selected -->
<div role="tab" aria-selected="true">Tab 1</div>

<!-- Checked -->
<div role="checkbox" aria-checked="true">Option</div>

<!-- Disabled -->
<button disabled aria-disabled="true">Save</button>

<!-- Invalid -->
<input aria-invalid="true" aria-errormessage="error-msg">
<div id="error-msg" role="alert">Email is required</div>

<!-- Required -->
<input required aria-required="true">

<!-- Current page -->
<a href="/home" aria-current="page">Home</a>

<!-- Hidden from screen readers -->
<div aria-hidden="true">Decorative content</div>
```

#### Live Regions (WCAG 4.1.3)

**Announce Dynamic Content**
```html
<!-- Polite: Wait for user to finish -->
<div role="status" aria-live="polite">
  5 new messages
</div>

<!-- Assertive: Interrupt immediately -->
<div role="alert" aria-live="assertive">
  Error: Connection lost
</div>

<!-- Atomic: Announce entire region -->
<div aria-live="polite" aria-atomic="true">
  Loading... 75% complete
</div>
```

**Usage Guidelines**
- Use sparingly
- Brief, meaningful messages
- Don't overuse assertive
- Clear old content before updating

### Alternative Text (WCAG 1.1.1)

#### Images

**Informative Images**
```html
<img src="chart.png" alt="Sales increased 25% in Q3 2025">
```

**Decorative Images**
```html
<img src="decoration.png" alt="">
<!-- or -->
<img src="decoration.png" role="presentation">
```

**Functional Images (links, buttons)**
```html
<a href="/home">
  <img src="home-icon.svg" alt="Home">
</a>
```

**Complex Images (charts, diagrams)**
```html
<figure>
  <img src="chart.png" alt="Sales data by quarter">
  <figcaption>
    <details>
      <summary>View data table</summary>
      <table><!-- accessible table data --></table>
    </details>
  </figcaption>
</figure>
```

#### Icons

**Icon with Text (decorative)**
```html
<button>
  <svg aria-hidden="true">...</svg>
  Save
</button>
```

**Icon without Text (functional)**
```html
<button aria-label="Save">
  <svg aria-hidden="true">...</svg>
</button>
```

#### Icon Fonts
- Use aria-hidden on icon element
- Provide text alternative
- Consider SVG instead for better support

### Headings & Structure (WCAG 1.3.1, 2.4.6)

**Heading Hierarchy**
```html
<h1>Page Title</h1>
  <h2>Section 1</h2>
    <h3>Subsection 1.1</h3>
    <h3>Subsection 1.2</h3>
  <h2>Section 2</h2>
    <h3>Subsection 2.1</h3>
```

**Guidelines**
- One H1 per page
- Don't skip heading levels
- Use for structure, not styling
- Descriptive heading text

### Form Accessibility

#### Labels (WCAG 1.3.1, 3.3.2)

**Always Provide Labels**
```html
✅ Good:
<label for="name">Full Name</label>
<input id="name" type="text">

❌ Bad:
<input type="text" placeholder="Full Name">
```

**Programmatic Association**
```html
<!-- Explicit label -->
<label for="email">Email</label>
<input id="email" type="email">

<!-- Implicit label (less preferred) -->
<label>
  Email
  <input type="email">
</label>

<!-- ARIA label (when visual label impossible) -->
<input type="search" aria-label="Search products">
```

#### Error Handling (WCAG 3.3.1, 3.3.3)

**Identify Errors**
```html
<label for="email">Email</label>
<input id="email" 
       type="email" 
       aria-invalid="true"
       aria-describedby="email-error">
<div id="email-error" role="alert">
  Please enter a valid email address
</div>
```

**Error Summary**
```html
<div role="alert" aria-labelledby="error-heading">
  <h2 id="error-heading">Please correct the following errors:</h2>
  <ul>
    <li><a href="#email">Email is required</a></li>
    <li><a href="#password">Password is too short</a></li>
  </ul>
</div>
```

**Guidelines**
- Identify which fields have errors
- Describe the error clearly
- Suggest how to fix
- Use color + icon + text
- Don't clear form on error

#### Input Instructions (WCAG 3.3.2)

**Provide Guidance**
```html
<label for="password">Password</label>
<input id="password" 
       type="password"
       aria-describedby="password-rules">
<div id="password-rules">
  Must be at least 8 characters with uppercase, lowercase, and number
</div>
```

#### Required Fields (WCAG 3.3.2)

**Indication Methods**
```html
<!-- Required attribute + visual indicator -->
<label for="name">
  Full Name <span aria-label="required">*</span>
</label>
<input id="name" type="text" required aria-required="true">

<!-- Legend explaining asterisk -->
<p><span aria-hidden="true">*</span> indicates required field</p>
```

---

## Touch & Mobile Accessibility

### Touch Target Size (WCAG 2.5.5)

**Minimum Size**
- 44x44 CSS pixels (iPhone guideline)
- 48x48 CSS pixels (Android guideline)
- Use 44x44 as minimum for both platforms

**Implementation**
```css
button {
  min-height: 44px;
  min-width: 44px;
  padding: 12px; /* if smaller visual size needed */
}
```

**Spacing**
- 8px minimum between touch targets
- More space for critical actions

### Gestures (WCAG 2.5.1)

**Path-based Gestures**
- Don't require complex gestures (drawing shapes)
- Provide simple alternative (tap, swipe)

**Multi-point Gestures**
- Don't require multi-finger gestures
- Provide single-point alternative
- Example: Pinch to zoom → Zoom buttons

**Alternative Input**
- Support mouse, touch, stylus, keyboard
- Don't assume specific input method

---

## Cognitive Accessibility

### Clear Language (WCAG 3.1.5)

**Guidelines**
- Use simple, clear language
- Explain technical terms
- Short sentences and paragraphs
- Active voice preferred
- Consistent terminology

### Reading Level
- Target 8th-9th grade reading level
- Use tools like Hemingway Editor
- Break up complex information

### Instructions (WCAG 3.3.2)

**Provide Clear Guidance**
- Explain form requirements before input
- Provide examples
- Show acceptable formats
- Use inline help and tooltips

### Error Prevention (WCAG 3.3.4)

**Confirmation for Critical Actions**
```html
<dialog role="alertdialog" aria-labelledby="confirm-title">
  <h2 id="confirm-title">Confirm Deletion</h2>
  <p>Are you sure you want to delete "Project X"? This cannot be undone.</p>
  <button>Cancel</button>
  <button>Delete</button>
</dialog>
```

**Guidelines**
- Confirm before destructive actions
- Allow review before submission
- Provide undo mechanism when possible
- Save drafts automatically

### Timeout Warnings (WCAG 2.2.1)

**Requirements**
- Warn before timeout
- Allow extension
- Provide at least 20 seconds to respond
- 10 times the default timeout duration for extension

---

## Testing & Validation

### Manual Testing

**Keyboard Testing**
1. Unplug mouse
2. Navigate with Tab, Arrow keys
3. Activate with Enter, Space
4. Verify focus indicators visible
5. Check no keyboard traps

**Screen Reader Testing**

**Windows**
- Narrator (built-in)
- NVDA (free)
- JAWS (commercial, most popular)

**macOS**
- VoiceOver (built-in)

**Mobile**
- TalkBack (Android)
- VoiceOver (iOS)

**Testing Checklist**
- [ ] All content announced
- [ ] Interactive elements identifiable
- [ ] Current state announced
- [ ] Form labels associated
- [ ] Error messages announced
- [ ] Dynamic content updates announced

**Zoom Testing**
- Test at 200% zoom (WCAG AA requirement)
- Test at 400% zoom (best practice)
- Verify no horizontal scrolling
- Content remains readable

**Color Contrast Testing**
- Test all text combinations
- Test UI component borders
- Test focus indicators
- Use contrast checker tools

### Automated Testing

**Tools**
- **axe DevTools**: Browser extension
- **Lighthouse**: Chrome DevTools
- **WAVE**: Browser extension
- **Accessibility Insights**: Windows app
- **Pa11y**: Command-line tool

**What Automated Tests Catch**
- Missing alt text
- Low contrast
- Missing labels
- Incorrect ARIA usage
- Missing landmarks
- Invalid HTML

**What Automated Tests Miss**
- Quality of alt text
- Logical heading structure
- Keyboard navigation flow
- Screen reader experience
- Context-specific issues

### User Testing

**Include Users With Disabilities**
- Screen reader users
- Keyboard-only users
- Low vision users
- Cognitive disabilities
- Motor impairments

**Testing Scenarios**
- Complete common tasks
- Navigate entire application
- Use forms and inputs
- Respond to errors
- Use on different devices

---

## Accessibility Checklist

### Development Checklist

- [ ] Valid, semantic HTML
- [ ] All images have appropriate alt text
- [ ] Color contrast meets WCAG AA (4.5:1 for text)
- [ ] All functionality keyboard accessible
- [ ] Visible focus indicators on all interactive elements
- [ ] Forms have associated labels
- [ ] Error messages clear and helpful
- [ ] ARIA attributes used correctly
- [ ] Headings in logical order
- [ ] Landmarks identify page regions
- [ ] Touch targets minimum 44x44px
- [ ] Supports zoom to 200%
- [ ] Respects reduced motion preference
- [ ] No keyboard traps
- [ ] Skip links provided
- [ ] Dynamic content uses live regions
- [ ] Modals trap focus appropriately
- [ ] Tables have proper headers
- [ ] Lists use proper markup
- [ ] No time limits or dismissible
- [ ] Confirmation for destructive actions

### Testing Checklist

- [ ] Keyboard navigation tested
- [ ] Screen reader tested (at least one)
- [ ] Zoom tested (200%, 400%)
- [ ] Automated testing run
- [ ] Color contrast verified
- [ ] Focus indicators visible
- [ ] Error states tested
- [ ] Mobile touch targets adequate
- [ ] User testing with disabilities completed

---

## Resources & Tools

### Guidelines & Standards

- [WCAG 2.1](https://www.w3.org/WAI/WCAG21/quickref/)
- [ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [Microsoft Inclusive Design](https://www.microsoft.com/design/inclusive/)
- [WebAIM Resources](https://webaim.org/resources/)

### Testing Tools

**Automated**
- [axe DevTools](https://www.deque.com/axe/devtools/)
- [Lighthouse](https://developers.google.com/web/tools/lighthouse)
- [WAVE](https://wave.webaim.org/)
- [Accessibility Insights](https://accessibilityinsights.io/)

**Contrast**
- [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [Colour Contrast Analyser](https://www.tpgi.com/color-contrast-checker/)

**Screen Readers**
- [NVDA](https://www.nvaccess.org/) (Windows, free)
- [JAWS](https://www.freedomscientific.com/products/software/jaws/) (Windows, commercial)
- Narrator (Windows, built-in)
- VoiceOver (macOS/iOS, built-in)
- TalkBack (Android, built-in)

### Learning Resources

**Courses**
- [Web Accessibility by Google (Udacity)](https://www.udacity.com/course/web-accessibility--ud891)
- [Introduction to Web Accessibility (W3C)](https://www.w3.org/WAI/fundamentals/accessibility-intro/)

**Communities**
- [WebAIM Discussion List](https://webaim.org/discussion/)
- [A11y Slack](https://web-a11y.slack.com/)
- [Accessibility Reddit](https://www.reddit.com/r/accessibility/)

---

## Maintenance

### Continuous Accessibility

**Every Release**
- Run automated tests
- Test keyboard navigation
- Verify contrast ratios
- Check new features with screen reader

**Quarterly**
- Full accessibility audit
- User testing with assistive technology users
- Update documentation

**Annually**
- Third-party accessibility audit
- Review against latest WCAG updates
- Training for development team

### Team Training

**Topics**
- WCAG standards overview
- Semantic HTML
- ARIA usage
- Screen reader basics
- Keyboard navigation
- Color contrast
- Testing techniques

**Resources**
- Monthly accessibility lunch-and-learn
- Accessibility champion on each team
- Documentation and examples
- Code review checklist

---

## Reporting Accessibility Issues

### User Reports

**Provide Clear Reporting Mechanism**
- Accessibility feedback form
- Email: accessibility@unigetui.com
- Contact information on every page

**Response Protocol**
- Acknowledge within 48 hours
- Provide workaround if possible
- Prioritize critical issues (P0)
- Communicate timeline for fix

### Issue Prioritization

**P0 (Critical)**
- Prevents core functionality
- No workaround available
- WCAG A violation

**P1 (High)**
- Significantly impacts experience
- Difficult workaround
- WCAG AA violation

**P2 (Medium)**
- Moderate impact
- Workaround available
- Usability issue

**P3 (Low)**
- Minor impact
- Enhancement
- WCAG AAA improvement

---

## Version Control

**Version**: 1.0.0  
**Last Updated**: November 2025  
**WCAG Version**: 2.1 Level AA  
**Maintained By**: UniGetUI Accessibility Team

## Related Documentation

- [Design System](./design-system.md)
- [Component Specifications](./component-specifications.md)
- [Design Tokens](./design-tokens.json)
