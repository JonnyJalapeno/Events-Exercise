using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ObjectiveC;
using System.Collections;
using System.Numerics;
using System.Collections.Immutable;

namespace EventExercise
{
    public class GuestEventArgs : EventArgs
    { 
        public CoatRoom CoatRoom { get; set; }
        public Guest Guest { get; set; }

        public GuestEventArgs(CoatRoom coatroom, Guest guest)
        {
            CoatRoom = coatroom;
            Guest = guest;
        }
    }

    public class Coat
    {
        private readonly string coatType;
        private readonly string coatOwner;

        public Coat(string coatOwner, string coatType)
        {
            this.coatType = coatType;
            this.coatOwner = coatOwner;
        }

        public string CoatType { get => this.coatType; }
        public string CoatOwner { get => this.coatOwner; }
    }

    public class Guest
    {
        private readonly string name;
        private Coat? coat;
        public Guest(string name, string? coatType = null)
        {
            this.name = name;
            if (coatType != null)
            {
                this.coat = new Coat(name, coatType);
            }
        }
        public string Name { get => this.name; }
        public Coat? Coat
        {
            get
            {
                if (coat != null)
                {
                    return coat;
                }
                else
                {
                    return null;
                }
            }
        }

        public void LeaveCoat()
        {
            coat = null;
        }

        public void GetCoat(Coat coat)
        {
            this.coat = coat;
        }
    }

    public class CoatRoom
    {
        private readonly List<Guest> guestList = new List<Guest>();
        private readonly List<Coat> coatList = new List<Coat>();
        public event EventHandler<GuestEventArgs>? GuestCameEvent;
        public event EventHandler<GuestEventArgs>? GuestLeftEvent;

        public void GuestCame(Guest guest)
        {
            guestList.Add(guest);
            OnGuestCame(guest);
        }

        public void GuestLeft(Guest guest)
        {
            guestList.Remove(guest);
            OnGuestLeft(guest);
        }

        public void AddCoat(Coat coat)
        {
            ArgumentNullException.ThrowIfNull(coat);
            coatList.Add(coat);
        }

        public void RemoveCoat(Coat coat)
        {
            coatList.Remove(coat);
        }

        public IReadOnlyList<Coat> RetrieveCoats()
        {
            return coatList.AsReadOnly();
        }

        protected virtual void OnGuestCame(Guest guest)
        {
            GuestEventArgs guestEventArgs = new GuestEventArgs(this, guest);
            GuestCameEvent?.Invoke(this, guestEventArgs);
        }
        protected virtual void OnGuestLeft(Guest guest)
        {
            GuestEventArgs guestEventArgs = new GuestEventArgs(this, guest);
            GuestLeftEvent?.Invoke(this, guestEventArgs);
        }
    }

    public static class Attendant
    {
        public static void GreetGuestAndTakeCoat(object? sender, GuestEventArgs e)
        {
            if (sender is CoatRoom coatroom)
            {
                Guest guest = e.Guest;
                Console.WriteLine($"Greetings {guest.Name}. Do you have any coat that I can take?");
                if (guest.Coat != null)
                {
                    coatroom.AddCoat(guest.Coat);
                    guest.LeaveCoat();
                    Console.WriteLine("Your coat has been deposited in the coat room. Enjoy your visit!");
                }
                else
                {
                    Console.WriteLine("Enjoy your visit!");
                }
            }
        }

        public static void FarewellGuestAndRemoveCoat(object? sender, GuestEventArgs e)
        {
            if (sender is CoatRoom coatroom) { 
                Guest guest = e.Guest;
                Console.WriteLine($"Welcome back {guest.Name}. Do you have any coat we need to return to you?");
                IReadOnlyList<Coat> coats = coatroom.RetrieveCoats();
                Coat? coat = coats.FirstOrDefault(x => x.CoatOwner == guest.Name);
                if (coat != null)
                {
                    coatroom.RemoveCoat(coat);
                    guest.GetCoat(coat);
                    Console.WriteLine("Here's your coat.Goodbye and we hope you've enjoyed the visit!");
                }
                else
                {
                    Console.WriteLine("Goodbye, and we hope you've enjoyed the visit!");
                }
            }
        }
    }

    static class Program
    {
        public static void Main()
        {
            CoatRoom coatroom = new CoatRoom();
            coatroom.GuestCameEvent += Attendant.GreetGuestAndTakeCoat;
            coatroom.GuestLeftEvent += Attendant.FarewellGuestAndRemoveCoat;
            Guest guest1 = new Guest("Albert Einstein");
            Guest guest2 = new Guest("Donald Trump", "Black Business Coat");
            Guest guest3 = new Guest("Michael Jackson", "Red Poncho Coat");
            coatroom.GuestCame(guest1);
            coatroom.GuestCame(guest2);
            coatroom.GuestCame(guest3);
            coatroom.GuestLeft(guest2);
            coatroom.GuestLeft(guest1);
        }
    }


}


