using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum BikePartPosition
{
    NONE,
    FrontWheelTire,
    RearWheelTire,
    FrontWheelTube,
    RearWheelTube,
    Saddle,
    Chain,
    FrontChainWheel,
    RearChainWheel,
    FrontBrakes,
    RearBrakes,
    Pedal,
}