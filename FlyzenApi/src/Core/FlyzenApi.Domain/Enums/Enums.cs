```csharp
namespace FlyzenApi.Domain.Enums
{
    public enum SeatClass
    {
        Economy,
        Business
    }

    public enum PassengerType
    {
        Adult,
        Child,
        Infant
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public enum MealType
    {
        Standard,
        Vegetarian,
        Vegan,
        Halal,
        GlutenFree
    }
}
```