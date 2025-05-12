using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


class HotelCapacity
{	
	private struct Event
	{
		public DateOnly Date { get; }
		public int Type { get; }

		public Event(DateOnly date, int type)
		{
			Date = date;
			Type = type;
		}
	}
	
	
	static bool CheckCapacity(int maxCapacity, List<Guest> guests)
	{
		List<Event> events = new List<Event>();

		foreach (var guest in guests)
		{
			var checkInDate = DateOnly.ParseExact(guest.CheckIn, "yyyy-MM-dd");
			var checkOutDate = DateOnly.ParseExact(guest.CheckOut, "yyyy-MM-dd");

			events.Add(new Event(checkInDate, 1));
			events.Add(new Event(checkOutDate, -1));
		}

		events.Sort((x, y) =>
		{ var temp = x.Date.CompareTo(y.Date);
		  if (temp != 0)
			  return temp;
		  return x.Type.CompareTo(y.Type); 
		});

		int numberOfGuests = 0;
		foreach (var eEvent in events)
		{
			numberOfGuests += eEvent.Type;
			if (numberOfGuests > maxCapacity)
			{
				return false;
			}
		}
		return true;
	}


	class Guest
	{
		public string Name { get; set; }
		public string CheckIn { get; set; }
		public string CheckOut { get; set; }
	}


	static void Main()
	{
		int maxCapacity = int.Parse(Console.ReadLine());
		int n = int.Parse(Console.ReadLine());


		List<Guest> guests = new List<Guest>();


		for (int i = 0; i < n; i++)
		{
			string line = Console.ReadLine();
			Guest guest = ParseGuest(line);
			guests.Add(guest);
		}


		bool result = CheckCapacity(maxCapacity, guests);


		Console.WriteLine(result ? "True" : "False");
	}


	// Простой парсер JSON-строки для объекта Guest
	static Guest ParseGuest(string json)
	{
		var guest = new Guest();


		// Извлекаем имя
		Match nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
		if (nameMatch.Success)
			guest.Name = nameMatch.Groups[1].Value;


		// Извлекаем дату заезда
		Match checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
		if (checkInMatch.Success)
			guest.CheckIn = checkInMatch.Groups[1].Value;


		// Извлекаем дату выезда
		Match checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
		if (checkOutMatch.Success)
			guest.CheckOut = checkOutMatch.Groups[1].Value;


		return guest;
	}
}
