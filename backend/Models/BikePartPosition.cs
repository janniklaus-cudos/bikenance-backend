using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum BikePartPosition
{
    NONE,
    FrontWheelTire,
    RearWheelTire,
    FrontWheelTube,
    RearWheelTube,
    FrontWheelRim,
    RearWheelRim,
    Saddle,
    Chain,
    FrontChainWheel,
    RearChainWheel,
    FrontBrakes,
    RearBrakes,
    BrakeHandles,
    Pedals,
    Grips,
    Frame,
}