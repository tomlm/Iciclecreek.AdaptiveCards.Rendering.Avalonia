# Adaptive Cards Rendering library for Avalonia UI
This library renders adaptive cards using [AvaloniaUI](https://avaloniaui.net/) controls.

# (Option 1) Use AdaptiveCardView control
Sample Model
```c#
public class CardModel
{
    public AdaptiveCard Card { get; set; }
}
```

Add namespace 
```xaml
 xmlns:ac="using:AdaptiveCards.Rendering.Avalonia"
```

Insert an AdaptiveCardView control bound to the card:
```xaml
<ac:AdaptiveCardView Card="{Binding Card}" Width="600" />
```

Produces a rendered card like this:
![image](https://github.com/tomlm/AdaptiveCards.Rendering.Avalonia/assets/17789481/0fb78aec-0b8b-4453-9ed4-39a511f6f6f2)

## Handling Actions
Actions are supported by the library, but you must provide a handler for the Action.Execute and Action.Submit actions.  To do that with the AdaptiveCardView 
you simply need to handle the Action event.  The event will be fired for all actions, so you will need to check the type of the action and handle it accordingly.
It is a routed event, so you can add the route handler at any level of the visual tree.

```xaml
   <ac:AdaptiveCardView Card="{Binding Card}" Width="600" Action="OnAdaptiveAction" />
```

And the action handler
```c#
    public static async void OnAdaptiveAction(Window window, object? sender, RoutedAdaptiveActionEventArgs e)
    {
        if (e.Action is AdaptiveOpenUrlAction openUrlAction)
        {
            Process.Start(new ProcessStartInfo(openUrlAction.Url.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        else if (e.IsDataAction())
        {
            // handle Action.Execute or Action.Submit
            ....
            e.Handled = true;
        }
   }
```

# Usage: (Option 2) Use AdaptiveCardRenderer directly 
You can call the renderer directly
```c#
var renderer = new AdaptiveCardRenderer(hostConfig);
var renderedCard = renderer.RenderCard(adaptiveCard);
renderedCard.OnAction += (sender, e) =>
{
    // handle event...
};

// add element to visual tree
grid.Children.Add(renderedCard.Control);
```

# Supported AdaptiveCard Schema
This library suppports all functionality for 1.5 of Adaptive Card schemas, and should work on all Avalonia platforms with following exceptions:

|Feature|Schema Version|Platform|Parsing|Rendering|
|---|---|---|:---:|:---:|
|Action.OpenUrl|v1.0|All| :white_check_mark: | :white_check_mark:|
|Action.ShowCard|v1.0|All| :white_check_mark: | :white_check_mark:|
|Action.Submit|v1.0|All| :white_check_mark: | :white_check_mark:|
|Column|v1.0|All| :white_check_mark: | :white_check_mark:|
|ColumnSet|v1.0|All| :white_check_mark: | :white_check_mark:|
|Container|v1.0|All| :white_check_mark: | :white_check_mark:|
|Fact|v1.0|All| :white_check_mark: | :white_check_mark:|
|FactSet|v1.0|All| :white_check_mark: | :white_check_mark:|
|Image|v1.0|All| :white_check_mark: | :white_check_mark:|
|ImageSet|v1.0|All| :white_check_mark: | :white_check_mark:|
|Input.ChoiceSet|v1.0|All| :white_check_mark: | :white_check_mark:|
|Input.Date|v1.0|All| :white_check_mark: | :white_check_mark:|
|Input.Number|v1.0|All| :white_check_mark: | :white_check_mark:|
|Input.Text|v1.0|All| :white_check_mark: | :white_check_mark:|
|Input.Time|v1.0|All| :white_check_mark: | :white_check_mark:|
|Input.Toggle|v1.0|All| :white_check_mark: | :white_check_mark:|
|SelectAction|v1.0|All| :white_check_mark: | :white_check_mark:|
|TextBlock|v1.0|All| :white_check_mark: | :white_check_mark:|
|Explicit Image Dimension|v1.1|All| :white_check_mark: | :white_check_mark:|
|Background Color|v1.1|All| :white_check_mark: | :white_check_mark:|
|Media|v1.1|Windows,MacOS,Linux| :white_check_mark: | :white_check_mark: |
|Vertical Content Alignment|v1.1|All| :white_check_mark: | :white_check_mark:|
|Action Icon|v1.1|All| :white_check_mark: | :white_check_mark:|
|Action Style|v1.2|All| :white_check_mark: | :white_check_mark:|
|Toggle Visibility|v1.2|All| :white_check_mark: | :white_check_mark:|
|ActionSet|v1.2|All| :white_check_mark: | :white_check_mark:|
|Fallback|v1.2|All| :white_check_mark: | :white_check_mark:|
|Container BackgroundImage|v1.2|All| :white_check_mark: | :white_check_mark:|
|Container MinHeight|v1.2|All| :white_check_mark: | :white_check_mark:|
|Container Bleed|v1.2|All| :white_check_mark: | :white_check_mark:|
|Container Style|v1.2|All| :white_check_mark: | :white_check_mark:|
|Image Data Uri|v1.2|All| :white_check_mark: | :white_check_mark:|
|Action Icon Data Uri|v1.2|All| :white_check_mark: | :white_check_mark:|
|Input.Text Inline Action|v1.2|All| :white_check_mark: | :white_check_mark:|
|TextBlock FontType|v1.2|All| :white_check_mark: | :white_check_mark:|
|RichTextBlock|v1.2|All| :white_check_mark: | :white_check_mark:|
|Input Label|v1.3|All| :white_check_mark: | :white_check_mark:|
|Input ErrorMessage|v1.3|All| :white_check_mark: | :white_check_mark:|
|AssociatedInputs|v1.3|All| :white_check_mark: | :white_check_mark:|
|RichTextBlock UnderLine|v1.3|All| :white_check_mark: | :white_check_mark:|
|Action.Refresh|v1.4|All| :white_check_mark: | :white_check_mark:|
|AdaptiveCard Authentication|v1.4|All| :white_check_mark: | :white_check_mark:|
|Action.Execute|v1.4|All| :white_check_mark: | :white_check_mark:|
|Action IsEnabled|v1.5|All| :white_check_mark: | :white_check_mark:|
|Action Mode|v1.5|All| :white_check_mark: | :white_check_mark:|
|Action/SelectAction ToolTip|v1.5|All| :white_check_mark: | :white_check_mark:|
|Input.ChoiceSet Filtered Style |v1.5|All| :white_check_mark: | :white_check_mark:|
|Input.Text Password Style |v1.5|All| :white_check_mark: | :white_check_mark:|
|TextBlock Heading Style|v1.5|All| :white_check_mark: | :white_check_mark:|
|RTL |v1.5|All| :white_check_mark: | :white_check_mark:|
|Table |v1.5|All| :white_check_mark: | :white_check_mark:|



