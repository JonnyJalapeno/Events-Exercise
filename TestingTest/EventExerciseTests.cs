using System;
using NUnit.Framework;
using System.Collections.Generic;
using EventExercise;

namespace EventExercise.Tests
{
    [TestFixture]
    public class CoatTests
    {
        [Test]
        public void Coat_StoresOwnerAndType_Correctly()
        {
            var coat = new Coat("Alice", "Trench Coat");

            Assert.That(coat.CoatOwner, Is.EqualTo("Alice"));
            Assert.That(coat.CoatType, Is.EqualTo("Trench Coat"));
        }
    }

    [TestFixture]
    public class GuestTests
    {
        [Test]
        public void Guest_WithNoCoat_HasNullCoat()
        {
            var guest = new Guest("Alice");

            Assert.That(guest.Coat, Is.Null);
        }

        [Test]
        public void Guest_WithCoat_HasCoat()
        {
            var guest = new Guest("Alice", "Leather Jacket");

            Assert.That(guest.Coat, Is.Not.Null);
            Assert.That(guest.Coat!.CoatType, Is.EqualTo("Leather Jacket"));
            Assert.That(guest.Coat.CoatOwner, Is.EqualTo("Alice"));
        }

        [Test]
        public void Guest_LeaveCoat_SetsCoatToNull()
        {
            var guest = new Guest("Alice", "Leather Jacket");
            guest.LeaveCoat();

            Assert.That(guest.Coat, Is.Null);
        }

        [Test]
        public void Guest_GetCoat_AssignsCoat()
        {
            var guest = new Guest("Alice");
            var coat = new Coat("Alice", "Parka");
            guest.GetCoat(coat);

            Assert.That(guest.Coat, Is.EqualTo(coat));
        }
    }

    [TestFixture]
    public class CoatRoomTests
    {
        private CoatRoom _coatRoom = null!;

        [SetUp]
        public void SetUp()
        {
            _coatRoom = new CoatRoom();
        }

        [Test]
        public void AddCoat_AddsCoatToRoom()
        {
            var coat = new Coat("Alice", "Trench Coat");
            _coatRoom.AddCoat(coat);

            Assert.That(_coatRoom.RetrieveCoats(), Contains.Item(coat));
        }

        [Test]
        public void AddCoat_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => _coatRoom.AddCoat(null!));
        }

        [Test]
        public void RemoveCoat_RemovesCoatFromRoom()
        {
            var coat = new Coat("Alice", "Trench Coat");
            _coatRoom.AddCoat(coat);
            _coatRoom.RemoveCoat(coat);

            Assert.That(_coatRoom.RetrieveCoats(), Does.Not.Contain(coat));
        }

        [Test]
        public void RetrieveCoats_ReturnsEmptyList_WhenNoCoatsAdded()
        {
            Assert.That(_coatRoom.RetrieveCoats(), Is.Empty);
        }

        [Test]
        public void RetrieveCoats_ReturnsAllAddedCoats()
        {
            var coat1 = new Coat("Alice", "Parka");
            var coat2 = new Coat("Bob", "Windbreaker");
            _coatRoom.AddCoat(coat1);
            _coatRoom.AddCoat(coat2);

            var coats = _coatRoom.RetrieveCoats();
            Assert.That(coats, Has.Count.EqualTo(2));
            Assert.That(coats, Contains.Item(coat1));
            Assert.That(coats, Contains.Item(coat2));
        }

        [Test]
        public void GuestCame_FiresGuestCameEvent()
        {
            bool eventFired = false;
            _coatRoom.GuestCameEvent += (sender, args) => eventFired = true;

            _coatRoom.GuestCame(new Guest("Alice"));

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void GuestLeft_FiresGuestLeftEvent()
        {
            bool eventFired = false;
            _coatRoom.GuestLeftEvent += (sender, args) => eventFired = true;

            var guest = new Guest("Alice");
            _coatRoom.GuestCame(guest);
            _coatRoom.GuestLeft(guest);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void GuestCameEvent_ReceivesCorrectGuest()
        {
            Guest? receivedGuest = null;
            _coatRoom.GuestCameEvent += (sender, args) => receivedGuest = args.Guest;

            var guest = new Guest("Alice");
            _coatRoom.GuestCame(guest);

            Assert.That(receivedGuest, Is.EqualTo(guest));
        }

        [Test]
        public void GuestLeftEvent_ReceivesCorrectGuest()
        {
            Guest? receivedGuest = null;
            _coatRoom.GuestLeftEvent += (sender, args) => receivedGuest = args.Guest;

            var guest = new Guest("Alice");
            _coatRoom.GuestCame(guest);
            _coatRoom.GuestLeft(guest);

            Assert.That(receivedGuest, Is.EqualTo(guest));
        }
    }

    [TestFixture]
    public class AttendantTests
    {
        private CoatRoom _coatRoom = null!;

        [SetUp]
        public void SetUp()
        {
            _coatRoom = new CoatRoom();
            _coatRoom.GuestCameEvent += Attendant.GreetGuestAndTakeCoat;
            _coatRoom.GuestLeftEvent += Attendant.FarewellGuestAndRemoveCoat;
        }

        [Test]
        public void GreetGuestAndTakeCoat_GuestWithCoat_CoatMovedToRoom()
        {
            var guest = new Guest("Alice", "Trench Coat");
            _coatRoom.GuestCame(guest);

            Assert.That(guest.Coat, Is.Null);
            Assert.That(_coatRoom.RetrieveCoats(), Has.Count.EqualTo(1));
            Assert.That(_coatRoom.RetrieveCoats()[0].CoatOwner, Is.EqualTo("Alice"));
        }

        [Test]
        public void GreetGuestAndTakeCoat_GuestWithoutCoat_NoCoatInRoom()
        {
            var guest = new Guest("Bob");
            _coatRoom.GuestCame(guest);

            Assert.That(_coatRoom.RetrieveCoats(), Is.Empty);
        }

        [Test]
        public void FarewellGuestAndRemoveCoat_GuestWithCoatInRoom_CoatReturnedToGuest()
        {
            var guest = new Guest("Alice", "Trench Coat");
            _coatRoom.GuestCame(guest);

            Assert.That(guest.Coat, Is.Null, "Coat should be in coat room after arrival");

            _coatRoom.GuestLeft(guest);

            Assert.That(guest.Coat, Is.Not.Null);
            Assert.That(guest.Coat!.CoatType, Is.EqualTo("Trench Coat"));
            Assert.That(_coatRoom.RetrieveCoats(), Is.Empty);
        }

        [Test]
        public void FarewellGuestAndRemoveCoat_GuestWithNoCoatInRoom_NoCoatAssigned()
        {
            var guest = new Guest("Bob");
            _coatRoom.GuestCame(guest);
            _coatRoom.GuestLeft(guest);

            Assert.That(guest.Coat, Is.Null);
            Assert.That(_coatRoom.RetrieveCoats(), Is.Empty);
        }

        [Test]
        public void MultipleGuests_EachGetCorrectCoatBack()
        {
            var guest1 = new Guest("Alice", "Trench Coat");
            var guest2 = new Guest("Bob", "Parka");
            var guest3 = new Guest("Charlie");

            _coatRoom.GuestCame(guest1);
            _coatRoom.GuestCame(guest2);
            _coatRoom.GuestCame(guest3);

            Assert.That(_coatRoom.RetrieveCoats(), Has.Count.EqualTo(2));

            _coatRoom.GuestLeft(guest1);
            Assert.That(guest1.Coat!.CoatType, Is.EqualTo("Trench Coat"));
            Assert.That(_coatRoom.RetrieveCoats(), Has.Count.EqualTo(1));

            _coatRoom.GuestLeft(guest2);
            Assert.That(guest2.Coat!.CoatType, Is.EqualTo("Parka"));
            Assert.That(_coatRoom.RetrieveCoats(), Is.Empty);

            _coatRoom.GuestLeft(guest3);
            Assert.That(guest3.Coat, Is.Null);
        }

        [Test]
        public void GuestEventArgs_ThrowOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GuestEventArgs(null,new Guest("Alice")));
        }

        [Test]
        public void GreetGuestAndTakeCoat_ThrowsOnNullCoatRoom()
        {
            CoatRoom coatroom = new CoatRoom();
            var args = new GuestEventArgs(coatroom, new Guest("Alice"));
            Assert.Throws<ArgumentNullException>(() => Attendant.GreetGuestAndTakeCoat(null, args));
        }

        [Test]
        public void FarewellGuestAndRemoveCoat_ThrowsOnNullCoatRoom()
        {
            CoatRoom coatroom = new CoatRoom();
            var args = new GuestEventArgs(coatroom, new Guest("Alice"));
            Assert.Throws<ArgumentNullException>(() => Attendant.FarewellGuestAndRemoveCoat(null, args));
        }
    }

    [TestFixture]
    public class GuestEventArgsTests
    {
        [Test]
        public void GuestEventArgs_StoresCoatRoomAndGuest()
        {
            var coatRoom = new CoatRoom();
            var guest = new Guest("Alice");
            var args = new GuestEventArgs(coatRoom, guest);

            Assert.That(args.CoatRoom, Is.EqualTo(coatRoom));
            Assert.That(args.Guest, Is.EqualTo(guest));
        }
    }
}
