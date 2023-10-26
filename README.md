# Adaptive Cards Rendering library for Avalonia UI
Adaptive Cards renderer for Avalonia (based on the WPF renderer)

# Usage
Sample Model
```c#
    public class CardModel
    {
        public AdaptiveCard Card { get; set; }
    }
}
```

View binding to the model
```xaml
  <Border Grid.Row="1" BorderThickness="2" CornerRadius="3" BoxShadow="5 5 10 0 Gray">
      <ac:AdaptiveCardView Card="{Binding Card}" Width="600" />
  </Border>
```
Produces a rendered card:
![image](https://github.com/tomlm/AdaptiveCards.Rendering.Avalonia/assets/17789481/0fb78aec-0b8b-4453-9ed4-39a511f6f6f2)
