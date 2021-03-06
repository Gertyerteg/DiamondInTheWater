﻿using DiamondInTheWater.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondInTheWater
{
    public class GameWorld
    {
        /// <summary>
        /// How many days you have until the end
        /// </summary>
        public const int RULE_DAYS = 60;

        public int Day
        {
            get;
            set;
        }

        public Nation[] Nations
        {
            get { return nations; }
        }

        public DayInfo LastDayStats
        {
            get { return GetPlayer().DayStats[GetPlayer().DayStats.Count - 1]; }
        }

        private Texture2D factoryTexture, houseTexture;
        private List<Factory> factories;
        private List<House> houses;
        private List<Person> persons;
        private List<Truck> trucks;
        private Random rand;
        private Texture2D islandTexture, blank, diamond;
        private Rectangle islandRectangle, worldBounds;
        private Nation[] nations;
        private Game1 game;
        public const int ISLAND_SCALE = 3;
        private ContentManager Content;
        private bool hasDiamond;

        /// <summary>
        /// Creates a new instance of the <c>GameWorld</c>.
        /// </summary>
        public GameWorld(Game1 game)
        {
            hasDiamond = true;
            this.game = game;
            rand = new Random();
            factories = new List<Factory>();
            houses = new List<House>();
            trucks = new List<Truck>();

            // Add different countries to TRADE with
            nations = new Nation[3];
            nations[0] = new Nation("Pelkeyland");
            nations[0].Chocolates = 2;
            nations[0].Phones = 2;
            nations[0].Shirts = 2;
            nations[1] = new Nation("The People's Republic of Berkeley");
            nations[1].Chocolates = 2;
            nations[1].Phones = 2;
            nations[1].Shirts = 2;
            nations[1].hasAIAdvantage = true;
            nations[2] = new Nation("The Sunnyvale Union");
            nations[2].Chocolates = 2;
            nations[2].Phones = 2;
            nations[2].Shirts = 2;
            nations[2].hasAIAdvantage = true;
            float[] consumerAdvantages = GenerateAdvantages();
            nations[0].ChocolateAdvantage = consumerAdvantages[0];
            nations[0].PhoneAdvantage = consumerAdvantages[1];
            nations[0].ShirtAdvantage = consumerAdvantages[2];
            nations[1].ChocolateAdvantage = consumerAdvantages[1];
            nations[1].PhoneAdvantage = consumerAdvantages[2];
            nations[1].ShirtAdvantage = consumerAdvantages[0];
            nations[2].ChocolateAdvantage = consumerAdvantages[2];
            nations[2].PhoneAdvantage = consumerAdvantages[0];
            nations[2].ShirtAdvantage = consumerAdvantages[1];

            float[] capitalAdvantages = GenerateAdvantages();
            //nations[0].FactoryAdvantage = capitalAdvantages[0];
            //nations[0].TruckAdvantage = capitalAdvantages[1];
            //nations[0].ToolAdvantage = capitalAdvantages[2];
            //nations[1].FactoryAdvantage = capitalAdvantages[1];
            //nations[1].TruckAdvantage = capitalAdvantages[2];
            //nations[1].ToolAdvantage = capitalAdvantages[0];
            //nations[2].FactoryAdvantage = capitalAdvantages[2];
            //nations[2].TruckAdvantage = capitalAdvantages[0];
            //nations[2].ToolAdvantage = capitalAdvantages[1];
            nations[0].FactoryAdvantage = 1;
            nations[0].TruckAdvantage = 1;
            nations[0].ToolAdvantage = 1;
            nations[1].FactoryAdvantage = 1;
            nations[1].TruckAdvantage = 1;
            nations[1].ToolAdvantage = 1;
            nations[2].FactoryAdvantage = 1;
            nations[2].TruckAdvantage = 1;
            nations[2].ToolAdvantage = 1;
        }

        private float[] GenerateAdvantages()
        {
            float[] advantages = new float[3];
            
            for (int i = 0; i < 3; i++)
            {
                int num = rand.Next(0, 3);

                while (advantages[num] != 0)
                {
                    num = rand.Next(0, 3);
                }
                advantages[num] = 0.5f + 0.25f * i;
            }
            
            return advantages;
        }

        public int[] GenerateStartingMaterials(int max)
        {
            int[] materials = new int[3];

            materials[0] = rand.Next(0, max);
            materials[1] = rand.Next(0, max - materials[0]);
            materials[2] = max - materials[0] - materials[1];

            return materials;
        }
        
        public Nation GetPlayer()
        {
            return nations[0];
        }

        /// <summary>
        /// Progresses the day.
        /// </summary>
        public void ProgressDay()
        {
            Nation n = GetPlayer();

            Day++;

            hasDiamond = (rand.Next(0, 100) < 40);

            //populationPrecise += (MAX_POPULATION * Math.Exp(Day)) / Math.Pow(1 + Math.Exp(Day) , 2);
            
            for (int i = 0; i < nations.Length; i++)
            {
                if (n != nations[i])
                    nations[i].DecideProduction();

                nations[i].ProgressProduction(Day);
                nations[i].ProgressPopulation(Day);
                nations[i].CalculateStatistics();
            }

            foreach (Nation nat in Nations)
                nat.ResetTrade();
        }

        public void Initialize(ContentManager Content)
        {
            Nation n = GetPlayer();
            this.Content = Content;
            houseTexture = Content.Load<Texture2D>("House");
            factoryTexture = Content.Load<Texture2D>("Factory");
            islandTexture = Content.Load<Texture2D>("Island");
            blank = Content.Load<Texture2D>("blank");
            diamond = Content.Load<Texture2D>("diamond");
            int isW = islandTexture.Width * ISLAND_SCALE;
            int isH = islandTexture.Height * ISLAND_SCALE;
            int gW = game.Width;
            int gH = game.Height;

            islandRectangle = new Rectangle(gW / 2 - isW / 2, gH / 2 - isH / 2, isW, isH);
            worldBounds = new Rectangle(islandRectangle.X + 26 * ISLAND_SCALE, islandRectangle.Y + 29 * ISLAND_SCALE,
                islandRectangle.Width - 53 * ISLAND_SCALE, islandRectangle.Height - 63 * ISLAND_SCALE);
            persons = new List<Person>();

            for (int i = 0; i < n.Population; i++)
            {
                Person p = new Person(worldBounds, rand);
                p.Position = new Vector2(rand.Next(worldBounds.X, worldBounds.X + worldBounds.Width),
                    rand.Next(worldBounds.Y, worldBounds.Y + worldBounds.Height));
                p.Initialize(Content);
                persons.Add(p);
            }
            for (int i = 0; i < (int)n.Trucks; i++)
            {
                Truck t = new Truck(worldBounds, rand);
                t.Position = new Vector2(rand.Next(worldBounds.X, worldBounds.X + worldBounds.Width),
                    rand.Next(worldBounds.Y, worldBounds.Y + worldBounds.Height));
                t.Initialize(Content);
                trucks.Add(t);
            }
            for (int i = 0; i < Math.Ceiling(n.Population / 4f); i++)
            {
                AddHouse();
            }
        }

        public void Update(GameTime gameTime)
        {
            Nation n = GetPlayer();
            foreach (Person p in persons)
                p.Update(gameTime);
            foreach (Truck t in trucks)
                t.Update(gameTime);

            while ((int)(n.Population / 4.2f) >= houses.Count)
                AddHouse();

            while ((int)(n.Population) >= persons.Count)
                AddPerson();

            while ((int)(n.Trucks) > trucks.Count)
                AddTruck();

            while ((int)n.Factories > factories.Count)
                AddFactory();
        }

        private void AddTruck()
        {
            Vector2 position = getRandomPositionWithinBounds();

            Truck t = new Truck(worldBounds, rand)
            {
                Position = new Vector2(rand.Next(worldBounds.X, worldBounds.X + worldBounds.Width),
                rand.Next(worldBounds.Y, worldBounds.Y + worldBounds.Height))
            };
            t.Initialize(Content);
            trucks.Add(t);
        }

        private void AddPerson()
        {
            Vector2 position = getRandomPositionWithinBounds();

            Person p = new Person(worldBounds, rand);
            p.Position = new Vector2(rand.Next(worldBounds.X, worldBounds.X + worldBounds.Width),
                rand.Next(worldBounds.Y, worldBounds.Y + worldBounds.Height));
            p.Initialize(Content);
            persons.Add(p);
        }

        /// <summary>
        /// Adds a new house to the island.
        /// </summary>
        public void AddHouse()
        {
            Vector2 position = getRandomPositionWithinBounds();

            House h = new House(position)
            {
                Texture = houseTexture
            };
            houses.Add(h);
        }
        private Vector2 getRandomPositionWithinBounds()
        {
            int tlCornerX = 34 * ISLAND_SCALE;
            int tlCornerY = 33 * ISLAND_SCALE;
            int brCornerX = 39 * ISLAND_SCALE;
            int brCornerY = 45 * ISLAND_SCALE;
            Vector2 bounds = new Vector2(islandRectangle.Width - tlCornerX - brCornerX, islandRectangle.Height - tlCornerY - brCornerY);
            Vector2 offset = new Vector2(islandRectangle.X + tlCornerX, islandRectangle.Y + tlCornerY);
            Vector2 position = new Vector2(rand.Next(0, (int)bounds.X) + offset.X,
                rand.Next(0, (int)bounds.Y) + offset.Y);

            return position;
        }

        /// <summary>
        /// Adds a new factory to the island.
        /// </summary>
        public void AddFactory()
        {
            Vector2 position = getRandomPositionWithinBounds();
            Factory f = new Factory(position)
            {
                Texture = factoryTexture
            };

            factories.Add(f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(islandTexture, islandRectangle, Color.White);
            //spriteBatch.Draw(blank, worldBounds, Color.White);

            foreach (Person p in persons)
                p.Draw(spriteBatch);

            foreach (Factory f in factories)
            {
                f.Draw(spriteBatch);
            }

            foreach (Truck t in trucks)
            {
                t.Draw(spriteBatch);
            }

            foreach (House h in houses)
                h.Draw(spriteBatch);
            
            if (hasDiamond)
            {
                spriteBatch.Draw(diamond, new Rectangle(game.Width - 90, 500, 78, 60), Color.White);
                Tile.tiles[1].Draw(spriteBatch, (game.Width - 80), 550, 64, 64);
            }

        }

        public void LoadGame(WorldSave save)
        {
            nations = save.Nations;
            Day = save.Day;
        }

        public WorldSave CreateSave()
        {
            WorldSave save = new WorldSave(nations, Day);
            return save;
        }
    }

    [Serializable]
    public struct WorldSave
    {
        public Nation[] Nations;
        public int Day;

        public WorldSave(Nation[] nations, int day)
        {
            Nations = nations;
            Day = day;
        }
    }

    [Serializable]
    public struct DayInfo
    {
        public float Production, Population, ProducedChocolate, ProducedShirts,
            ProducedPhones, ProducedFactories, ProducedTrucks, ProducedTools, TradeChocolate,
            TradeShirt, TradePhone, BoughtChocolate, BoughtShirt, BoughtPhone, TotalShirts, TotalChocolates,
            TotalPhones, TotalFactory, TotalTrucks, TotalTools;

        public DayInfo(float tS, float tC, float tP, float tF, float tT, float tTo, float production, float population, float producedChocolate, float producedShirts, float producedPhones,
            float producedFactories, float producedTrucks, float producedTools, float tradeChocolate, float tradeShirt, float tradePhone,
            float boughtChocolate, float boughtShirt, float boughtPhone)
        {
            TotalShirts = tS;
            TotalChocolates = tC;
            TotalPhones = tP;
            TotalFactory = tF;
            TotalTrucks = tT;
            TotalTools = tTo;

            Production = production;
            Population = population;

            ProducedChocolate = producedChocolate;
            ProducedShirts = producedShirts;
            ProducedPhones = producedPhones;
            ProducedFactories = producedFactories;
            ProducedTrucks = producedTrucks;
            ProducedTools = producedTools;
            TradeChocolate = tradeChocolate;
            TradeShirt = tradeShirt;
            TradePhone = tradePhone;
            BoughtChocolate = boughtChocolate;
            BoughtPhone = boughtPhone;
            BoughtShirt = boughtShirt;
        }
    }
}
