using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpellFall.Background;
using System;

namespace SpellFall.Engine
{
    public class World
    {
        public Dictionary<Point, Room> Rooms = new();
        private Random _random = new Random();
        public void Generate(int maxRooms)
        {
            Point current = Point.Zero;
            Rooms[current] = CreateRoom();

            for (int i = 0; i < maxRooms; i++)
            {
                Point dir = GetRandomDirection();
                Point next = current + dir;

                if (!Rooms.ContainsKey(next))
                {
                    var newRoom = CreateRoom();

                    ConnectRooms(Rooms[current], newRoom, dir);

                    Rooms[next] = newRoom;
                    current = next;
                }
            }
            foreach (var room in Rooms.Values)
            {
                room.GenerateRoom();
            }
        }

        private Point GetRandomDirection()
        {
            int r = _random.Next(4);

            return r switch
            {
                0 => new Point(0, -1),
                1 => new Point(0, 1),
                2 => new Point(-1, 0),
                _ => new Point(1, 0),
            };
        }

        void ConnectRooms(Room a, Room b, Point dir)
        {
            if (dir == new Point(0, -1)) { a.DoorNorth = true; b.DoorSouth = true; }
            if (dir == new Point(0, 1))  { a.DoorSouth = true; b.DoorNorth = true; }
            if (dir == new Point(-1, 0)) { a.DoorWest = true;  b.DoorEast = true; }
            if (dir == new Point(1, 0))  { a.DoorEast = true;  b.DoorWest = true; }

            a.GenerateRoom();
            b.GenerateRoom();
        }
        private Room CreateRoom()
        {
            return new Room();
        }
    }
}