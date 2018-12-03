﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondInTheWater.Entities
{
    [Serializable]
    public class Nation
    {
        public readonly string Name;

        public float FactoryAdvantage, ToolAdvantage, TruckAdvantage,
            PhoneAdvantage, ChocolateAdvantage, ShirtAdvantage;

        public float TradeChocolates, TradePhones, TradeShirts, BoughtChocolates, BoughtPhones, BoughtShirts;
        public float QueuedFactories
        {
            get;
            set;
        }
        public float QueuedTrucks
        {
            get;
            set;
        }
        public float QueuedTools
        {
            get;
            set;
        }

        public float QueuedShirts
        {
            get;
            set;
        }

        public float QueuedPhones
        {
            get;
            set;
        }

        public float QueuedChocolates
        {
            get;
            set;
        }

        public float Factories
        {
            get;
            set;
        }
        public float Trucks
        {
            get;
            set;
        }
        public float Tools
        {
            get;
            set;
        }

        public float Shirts
        {
            get;
            set;
        }

        public float Chocolates
        {
            get;
            set;
        }

        public float Phones
        {
            get;
            set;
        }

        public bool hasAIAdvantage
        {
            get;
            set;
        }
        public int Coco
        {
            get { return (int)(ChocolateAdvantage * 33); }
        }

        public int Silk
        {
            get { return (int)(ShirtAdvantage * 31); }
        }

        public int Materials
        {
            get { return (int)(PhoneAdvantage * 34); }
        }

        public float Population
        {
            get { return (int)populationPrecise; }
        }

        public float Production
        {
            get { float realProd = (int)Population + 
                    ((int)Tools * TOOL_NEEDED + (int)Factories * FACTORY_NEEDED 
                    + (int)Trucks * TRUCK_NEEDED) / 300 * (1 - Unemployment) + 40f;
                float adProd = (hasAIAdvantage) ? realProd * 1.2f + 50f : realProd;
                return adProd;
            }
        }

        public float Unemployment
        {
            get;
            set;
        }

        public float RealUnemployment
        {
            get { return Unemployment; }
        }

        public float QueuedGoods
        {
            get { return (float)(Math.Ceiling(QueuedChocolates) + Math.Ceiling(QueuedFactories) + 
                    Math.Ceiling(QueuedPhones) + Math.Ceiling(QueuedShirts) + 
                    Math.Ceiling(QueuedTools) + Math.Ceiling(QueuedTrucks)); }
        }
        
        public const float FACTORY_PRODUCTION = 200f;
        public const int MAX_POPULATION = 100;

        public const float FACTORY_NEEDED = 320;
        public const float TRUCK_NEEDED = 120;
        public const float TOOL_NEEDED = 70;
        public const float SHIRT_NEEDED = 80;
        public const float CHOC_NEEDED = 60;
        public const float PHONE_NEEDED = 110;
        private double populationPrecise;

        public List<DayInfo> DayStats
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Creates a new <c>Nation</c>.
        /// </summary>
        public Nation(string name)
        {
            DayStats = new List<DayInfo>();
            Unemployment = 0.05f;
            Name = name;
            populationPrecise = 32.0;
            DayStats.Add(new DayInfo(0, 0, 0, 0, 0, 0, Production, (float)populationPrecise, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
            hasAIAdvantage = false;
        }

        public void ResetTrade()
        {
            BoughtChocolates = BoughtPhones = BoughtShirts = TradeChocolates = TradePhones = TradeShirts = 0f;
        }

        public bool IsGoodTrade(float theirChocolate, float theirPhone, float theirShirt,
            float yourChocolate, float yourPhone, float yourShirt)
        {
            float chocValue = ChocolateValue(Chocolates + theirChocolate - yourChocolate)
                - ChocolateValue();
            float phoneValue = PhoneValue(Phones + theirPhone - yourPhone)
                - PhoneValue();
            float shirtValue = ShirtValue(Shirts + theirShirt - yourShirt)
                - ShirtValue();
            //float tradeValue = (theirChocolate / ChocolateAdvantage * CHOC_NEEDED + theirPhone
            //    / PhoneAdvantage * PHONE_NEEDED + theirShirt / ShirtAdvantage * SHIRT_NEEDED) 
            //    - (yourChocolate / ChocolateAdvantage * CHOC_NEEDED + yourPhone / PhoneAdvantage 
            //    * PHONE_NEEDED + yourShirt / ShirtAdvantage * SHIRT_NEEDED);

            float tradeValue = chocValue + phoneValue + shirtValue;
            
            return tradeValue > 0;
        }

        public void DecideProduction()
        {
            QueuedChocolates += (float)Math.Pow(ChocolateAdvantage, 3) * Production / 1.5f;
            QueuedPhones += (float)Math.Pow(PhoneAdvantage, 3) * Production / 1.5f;
            QueuedShirts += (float)Math.Pow(ShirtAdvantage , 3) * Production / 1.5f;
            QueuedFactories += FactoryAdvantage * Production / 2f;
            QueuedTools += ToolAdvantage * Production / 2f;
            QueuedTrucks += TruckAdvantage * Production / 2f;
        }

        public double InflationMult
        {
            get { return 1 / Math.Max(RealUnemployment, 0.05); }
        }

        public bool IsProducing()
        {
            float queued = QueuedChocolates + QueuedFactories + QueuedPhones + QueuedShirts + QueuedTools + QueuedTrucks;

            return queued > 0;
        }

        public void CalculateStatistics()
        {
            // calculates the total GDP of the nation for that day
            float consumerGoods = Chocolates * CHOC_NEEDED + Phones *
                PHONE_NEEDED + Shirts * SHIRT_NEEDED; // CONSUMPTION
            float govSpending = 0f; // FISCAL POLICY AND GOVERNMENT SPENDING
            float capitalGoods = Factories * FACTORY_NEEDED + Trucks *
                TRUCK_NEEDED + Tools * TOOL_NEEDED; // INVESTMENT
            float exports = 0f; // IMPLEMENT TRADING
            float imports = 0f; // IMPLEMENT IMPORTS
            float GDP = consumerGoods + govSpending + capitalGoods + exports + imports;
            DayStats.Add(new DayInfo(Shirts, Chocolates, Phones, Factories, Trucks, Tools,
                Production, Population, Chocolates, Shirts,
                Phones, Factories, Trucks, Tools, TradeChocolates, TradeShirts,
                TradePhones, BoughtChocolates, BoughtShirts, BoughtPhones));
            Console.WriteLine(Population);
        }

        public float CalculateHappiness()
        { 
            return (float)(Math.Max(Math.Log(Chocolates * Phones * Shirts / 2), 1)) * (1 - RealUnemployment) * 0.45f;
        }

        public float ChocolateValue(float choc)
        {
            return choc * CHOC_NEEDED / ChocolateAdvantage;
        }

        public float ChocolateValue()
        {
            return ChocolateValue(Chocolates);
        }

        public float ShirtValue()
        {
            return ShirtValue(Shirts);
        }

        public float PhoneValue()
        {
            return PhoneValue(Phones);
        }

        public float ShirtValue(float shirt)
        {
            return shirt * SHIRT_NEEDED / ShirtAdvantage;
        }
        public float PhoneValue(float phone)
        {
            return phone * PHONE_NEEDED / PhoneAdvantage;
        }

        //public float ChocolateValue(float choc)
        //{
        //    float proportion = Math.Min(choc / (Chocolates + Phones + Shirts), 0.75f);

        //    float realValue = choc * CHOC_NEEDED / ChocolateAdvantage * (1 - proportion);

        //    return realValue;
        //}

        //public float ChocolateValue()
        //{
        //    return ChocolateValue(Chocolates);
        //}

        //public float PhoneValue(float phon)
        //{
        //    float proportion = Math.Min(phon / (Chocolates + Phones + Shirts), 0.75f);

        //    float realValue = (phon * PHONE_NEEDED / PhoneAdvantage) * (1 - proportion);

        //    return realValue;
        //}

        //public float PhoneValue()
        //{
        //    return PhoneValue(Phones);
        //}

        //public float ShirtValue(float shirt)
        //{
        //    float proportion = Math.Min(shirt / (Chocolates + Phones + Shirts), 0.75f);

        //    float realValue = shirt * SHIRT_NEEDED / ShirtAdvantage * (1 - proportion);

        //    return realValue;
        //}

        //public float ShirtValue()
        //{
        //    return ShirtValue(Shirts);
        //}

        public void ProgressPopulation(int Day)
        {
            float step = 30f;
            float x = Day / step;
            float happiness = CalculateHappiness();
            populationPrecise += MAX_POPULATION * (Math.Exp(x) /
                (Math.Pow(Math.Exp(x) + 1, 2))) / step * happiness;
        }

        public double CalculateValue(Goods good, int initial, int final)
        {
            double prod = 0;
            double adv = 0;
            switch (good)
            {
                case Goods.TOOL:
                    prod = TOOL_NEEDED;
                    adv = ToolAdvantage;
                    break;
                case Goods.FACTORY:
                    prod = FACTORY_NEEDED;
                    adv = FactoryAdvantage;
                    break;
                case Goods.TRUCK:
                    prod = TRUCK_NEEDED;
                    adv = TruckAdvantage;
                    break;
                case Goods.CHOCOLATE:
                    prod = CHOC_NEEDED;
                    adv = ChocolateAdvantage;
                    break;
                case Goods.SHIRT:
                    prod = SHIRT_NEEDED;
                    adv = ShirtAdvantage;
                    break;
                case Goods.PHONE:
                    prod = PHONE_NEEDED;
                    adv = PhoneAdvantage;
                    break;
            }
            double value = 0;
            for (int i = 0; i < final - initial; i++)
            {
                value += (Math.Pow(2, .1) * prod) / Math.Pow(i + initial + 2, .1);
            }

            return value / adv;
        }

        public double CalculateTotal(Goods good)
        {
            int goods = 0;
            switch (good)
            {
                case Goods.TOOL:
                    goods = (int)Tools;
                    break;
                case Goods.FACTORY:
                    goods = (int)Factories;
                    break;
                case Goods.TRUCK:
                    goods = (int)Trucks;
                    break;
                case Goods.CHOCOLATE:
                    goods = (int)Chocolates;
                    break;
                case Goods.SHIRT:
                    goods = (int)Shirts;
                    break;
                case Goods.PHONE:
                    goods = (int)Phones;
                    break;
            }
            return CalculateValue(good, 0, goods);
        }

        public void ProgressProduction(int Day)
        {
            //float queuedGoods = (float)(Math.Ceiling(QueuedFactories) / FactoryAdvantage + Math.Ceiling(QueuedPhones)
            //    + Math.Ceiling(QueuedTrucks) / TruckAdvantage + Math.Ceiling(QueuedChocolates)
            //    + Math.Ceiling(QueuedShirts) + Math.Ceiling(QueuedTools) / ToolAdvantage);

            float dividedProd = Production / QueuedGoods;

            if (QueuedFactories > 0)
            {
                float delta = (float)(dividedProd / FACTORY_NEEDED * Math.Ceiling(QueuedFactories)) * FactoryAdvantage;
                //Factories += (int)(QueuedFactories) - (int)(QueuedFactories - delta);
                Factories += delta;
                QueuedFactories -= delta;
            }
            if (QueuedFactories < 0)
                QueuedFactories = 0;

            if (QueuedTrucks > 0)
            {
                float delta = (float)(dividedProd / TRUCK_NEEDED * Math.Ceiling(QueuedTrucks)) * TruckAdvantage;
                //Trucks += (int)(QueuedTrucks) - (int)(QueuedTrucks - delta);
                Trucks += delta;
                QueuedTrucks -= delta;
            }
            if (QueuedTrucks < 0)
                QueuedTrucks = 0;
            if (QueuedTools > 0)
            {
                float delta = (float)(dividedProd / TOOL_NEEDED * Math.Ceiling(QueuedTools)) * ToolAdvantage;
                //Tools += (int)(QueuedTools) - (int)(QueuedTools - delta);
                Tools += delta;
                QueuedTools -= delta;
            }
            if (QueuedTools < 0)
                QueuedTools = 0;

            if (QueuedChocolates > 0)
            {
                float delta = (float)(dividedProd / CHOC_NEEDED * Math.Ceiling(QueuedChocolates)) * ChocolateAdvantage;
                //Chocolates += (int)(QueuedChocolates) - (int)(QueuedChocolates - delta);
                Chocolates += delta;
                QueuedChocolates -= delta;
            }
            if (QueuedChocolates < 0)
                QueuedChocolates = 0;

            if (QueuedPhones > 0)
            {
                float delta = (float)(dividedProd / PHONE_NEEDED * Math.Ceiling(QueuedPhones)) * PhoneAdvantage;
                //Phones += (int)(QueuedPhones) - (int)(QueuedPhones - delta);
                Phones += delta;
                QueuedPhones -= delta;
            }
            if (QueuedPhones < 0)
                QueuedPhones = 0;

            if (QueuedShirts > 0)
            {
                float delta = (float)(dividedProd / SHIRT_NEEDED * Math.Ceiling(QueuedShirts)) * ShirtAdvantage;
                //Shirts += (int)(QueuedShirts) - (int)(QueuedShirts - delta);
                Shirts += delta;

                QueuedShirts -= delta;
            }
            if (QueuedShirts < 0)
                QueuedShirts = 0;
        }
    }
}
