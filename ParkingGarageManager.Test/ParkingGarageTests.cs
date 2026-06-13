using Xunit;
using ParkingGarageManager;
using Xunit.Sdk;
using System.ComponentModel;

namespace ParkingGarageManager.Test;

public class ParkingGarageTests
{

    private Core CreateCore()
    {
        var core = new Core(skipLoad: true);
        core.ParkingGarage.Spots["A01"] = new OpenSpot("A01");
        core.ParkingGarage.Spots["A02"] = new OpenSpot("A02");
        core.ParkingGarage.Spots["A03"] = new OpenSpot("A03");
        return core;
    }

    // Check-in / Check-out spot tests

    [Fact]
    public void CheckIn_OpenSpot_SetsSpotToTaken()
    {
        var core = CreateCore();
        core.ParkingGarage.Spots["A01"] = new OpenSpot("A01");
        core.Vehicles["NY-0222"] = new Vehicle("NY-0222");

        core.CheckInManager.CheckInSpot("A01", "NY-0222");

        Assert.IsType<TakenSpot>(core.ParkingGarage.Spots["A01"]);
    }

    [Fact]
    public void CheckOut_TakenSpot_SetsSpotToOpen()
    {
        var core = CreateCore();
        var vehicle = new Vehicle("NY-0222");
        core.Vehicles["NY-0222"] = vehicle;
        core.ParkingGarage.Spots["A01"] = new TakenSpot("A01", vehicle);
        core.CheckInManager.CreateVisit("NY-0222", "A01");

        var (_, _, spotID, subscriber) = core.CheckOutManager.CheckOutSpot("NY-0222");
        core.SpotManager.RestoreSpot(spotID, subscriber as ActiveSubscriber);

        Assert.IsType<OpenSpot>(core.ParkingGarage.Spots["A01"]);
    }

    [Fact]
    public void CheckIn_AlreadyTakenSpot_ThrowsInvalidOperationException()
    {
        var core = CreateCore();
        var vehicle = new Vehicle("NY-0222");
        core.Vehicles["NY-0222"] = vehicle;
        core.ParkingGarage.Spots["A01"] = new TakenSpot("A01", vehicle);

        Assert.Throws<InvalidOperationException>(() => core.CheckInManager.CheckInSpot("A01", "NY-0222"));
    }

    [Fact]
    public void CheckIn_SpotDoesNotExist_ThrowsKeyNotFoundException()
    {
        var core = CreateCore();
        core.Vehicles["NY-0222"] = new Vehicle("NY-0222");

        Assert.Throws<KeyNotFoundException>(() => core.CheckInManager.CheckInSpot("Z01", "NY-0222"));
    }

    [Fact]
    public void CheckOut_VehicleNotCheckedIn_ThrowsKeyNotFoundException()
    {
        var core = CreateCore();
        core.Vehicles["NY-0222"] = new Vehicle("NY-0222");

        Assert.Throws<KeyNotFoundException>(() => core.CheckOutManager.CheckOutSpot("NY-0222"));
    }

    // Subscriber Tests

    [Fact]
    public void AddSubscriber_ValidData_AddsActiveSubscriber()
    {
        var core = CreateCore();
        core.ParkingGarage.Spots["A01"] = new OpenSpot("A01");

        core.SubscriberManager.AddSubscriber(
            DateOnly.FromDateTime(DateTime.Now),
            "TEST123",
            "John Doe",
            "johndoe@testemail.com",
            licensePlateNumbers: new List<string> {"NY-0222"},
            spotIDs: new List<string> {"A01"}
        );

        Assert.Single(core.Subscribers);
        Assert.IsType<ActiveSubscriber>(core.Subscribers.Values.First());
    }

    [Fact]
    public void AddSubscriber_DuplicateDriversLicense_ThrowsInvalidOperationsException()
    {
        var core = CreateCore();
        core.ParkingGarage.Spots["A01"] = new OpenSpot("A01");
        core.ParkingGarage.Spots["A02"] = new OpenSpot("A02");

        core.SubscriberManager.AddSubscriber(
            DateOnly.FromDateTime(DateTime.Now),
            "TEST123",
            "John Smith",
            "johnsmith@testemail.com",
            licensePlateNumbers: new List<string> {"NY-0222"},
            spotIDs: new List<string> {"A01"}
        );

        Assert.Throws<InvalidOperationException>(() =>
            core.SubscriberManager.AddSubscriber(
                DateOnly.FromDateTime(DateTime.Now),
                "TEST123",
                "Jane Doe",
                "jane@email.com",
                licensePlateNumbers: new List<string> { "NJ-X114" },
                spotIDs: new List<string> { "A02" }
            )
        );
    }

    [Fact]
    public void AddSubscriber_DuplicateLicensePlateNumbers_InvalidOperationException()
    {
        var core = CreateCore();

        core.ParkingGarage.Spots["A01"] = new OpenSpot("A01");
        core.ParkingGarage.Spots["A02"] = new OpenSpot("A02");

        core.SubscriberManager.AddSubscriber(
            DateOnly.FromDateTime(DateTime.Now),
            "TEST123",
            "John Doe",
            "johnd@testemail.com",
            licensePlateNumbers: new List<string> {"NY-0222"},
            spotIDs: new List<string> {"A01"}
        );

        Assert.Throws<InvalidOperationException>(() =>
            core.SubscriberManager.AddSubscriber(
                DateOnly.FromDateTime(DateTime.Now),
                "123TEST",
                "Jane Doe",
                "janed@testemail.com",
                licensePlateNumbers: new List<string> {"NY-0222"},
                spotIDs: new List<string> {"A02"}
            )
        );
    }

    [Fact]
    public void AddSubscriber_SetsSpotToReserved()
    {
        var core = CreateCore();
        core.ParkingGarage.Spots["A01"] = new OpenSpot("A01");

        core.SubscriberManager.AddSubscriber(
            DateOnly.FromDateTime(DateTime.Now),
            "TEST123",
            "John Doe",
            "johnd@testemail.com",
            licensePlateNumbers: new List<string> {"NY-0222"},
            spotIDs: new List<string> {"A01"}
        );

        Assert.IsType<ReservedSpot>(core.ParkingGarage.Spots["A01"]);
    }

    // Fee calculation tests

    [Fact]
    public void EndVisit_OneHourStay_CalculatesCorrectFee()
    {
        string visitID = "1";
        DateTime entryTime = DateTime.Now.AddHours(-1);
        var visit = new Visit(visitID, "A01",entryTime, Core.HourlyRate);

        PendingPayment? payment = visit.EndVisit("1", isSubscriber: false);

        Assert.NotNull(payment);
        Assert.Equal(Core.HourlyRate, payment!.AmountOwed);
    }

    [Fact]
    public void EndVisit_PartialHourStay_RoundsUpToNextHour()
    {
        string visitID = "1";
        DateTime entryTime = DateTime.Now.AddMinutes(-90);
        var visit = new Visit(visitID, "A01", entryTime, Core.HourlyRate);

        PendingPayment? payment = visit.EndVisit("1", isSubscriber: false);

        Assert.NotNull(payment);
        Assert.Equal(Core.HourlyRate * 2, payment!.AmountOwed);
    }

    [Fact]
    public void EndVisit_SubscriberStay_ReturnsNullPayment()
    {
        string visitID = "1";
        DateTime entryTime = DateTime.Now.AddHours(-2);
        var visit = new Visit(visitID, "A01", entryTime, Core.HourlyRate);

        PendingPayment? payment = visit.EndVisit("1", isSubscriber: true);

        Assert.Null(payment);
    }
}
