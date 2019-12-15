﻿using System;

namespace Kibol_Alert.Models
{
    public class Brawl
    {
        public int Id { get; set; }
        public Club FirstClub { get; set; }
        public int FirstClubId { get; set; }
        public Club SecondClub { get; set; }
        public int SecondClubId { get; set; }
        public DateTime Date { get; set; }
        public Location Location { get; set; }
    }
}
