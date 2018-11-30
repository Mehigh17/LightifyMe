# LightifyMe

This library gives you the power to communicate with your Osram Lightify light bulbs. You can control their brightness, color, status and get the metadata such as bulb temperature, name and so on. The library was built with .NET Core 2.1 so you can easily use it on your Mac or Linux machine.

# Usage

## Initialize your gateway

The gateway is used to communicate with your bulbs, you use it for every action you want to execute on any bulb.

```csharp
var gateway = new GatewayController();
await gateway.Init("gateway_local_ip");
```

There are various methods to obtain your gateway ip address, the easiest one being by checking your router home panel where all the connected devices are displayed.

## Fetch all light bulbs

```csharp
var bulbs = gateway.GetBulbs();
```

Keep in mind that the data isn't updated by some magic handler behind the controller. You fetch the bulb data at time *x* and if you want an update you must fetch it again. This is the only solution available right now with this library.

## Turn on and off a bulb

```csharp
gateway.TurnOn(bulb); // Turn on your light bulb
gateway.TurnOff(bulb); // Turn off your light bulb
```

These two functions will update your bulb object status. So the property `IsOn` will be flipped according to the case;

## Mass turn on/off

```csharp
gateway.TurnAllOn(); // Turn all the available lightsbulbs on
gateway.TurnAllOff(); // Turn all the available lightsbulbs off
```

If you've already fetched your light bulb objects before, keep in mind this function won't update your objects `IsOn` property  like the independent functions!

## Change color

```csharp
var color = Color.FromArgb(255, 123, 182, 83); // Or Color.DodgerBlue
gateway.SetColor(bulb, color);
```

This function will update your light bulb `Color` property aswell.

## Change brightness

```csharp
var brigthnessPercentage = 48; // Must be between [0; 100]
gateway.SetBrightness(bulb, brigthnessPercentage);
```

This function will update your light bulb `Brightness` property aswell.

# Status

Keep in mind this library is still young and experimental so things might broke.