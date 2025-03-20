# Value Converters

This document describes the value converters used in the Kumano Kodo Japanese Learning Application.

## NullToVisibilityConverter

The `NullToVisibilityConverter` is a one-way value converter that transforms null values into WPF Visibility values. This is particularly useful for conditionally showing or hiding UI elements based on whether their bound data is null.

### Usage

```xaml
<Image Source="{Binding ImageUrl}" 
       Visibility="{Binding ImageUrl, Converter={StaticResource NullToVisibilityConverter}}"/>
```

### Behavior

- When the bound value is `null`: Returns `Visibility.Collapsed`
- When the bound value is not `null`: Returns `Visibility.Visible`

### Implementation Details

- Implements `IValueConverter`
- Only supports one-way conversion (from source to target)
- Throws `InvalidOperationException` if target type is not `Visibility`
- Throws `NotSupportedException` for convert-back operations

### Common Use Cases

1. Hiding media elements (images, audio) when no source URL is available
2. Hiding detail views when no item is selected
3. Managing visibility of optional UI elements based on data availability

### Example Locations

The converter is used in several views:

1. `LessonsPage.xaml`:
   - Lesson details section visibility
   - Media elements visibility
   - Action button visibility

2. `ProgressPage.xaml`:
   - Progress map image visibility

## Registration

All converters are registered as resources in `App.xaml`:

```xaml
<Application.Resources>
    <ResourceDictionary>
        <converters:NullToVisibilityConverter x:Key="NullToVisibilityConverter"/>
    </ResourceDictionary>
</Application.Resources>
``` 