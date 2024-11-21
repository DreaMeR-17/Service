using System;
using System.Collections.Generic;

namespace Service
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DetailFactory detailFactory = new DetailFactory();
            WarehouseFactory warehouseFactory = new WarehouseFactory();
            CarFactory carFactory = new CarFactory(detailFactory.Create());
            ServiceFactory serviceFactory = new ServiceFactory();

            Warehouse warehouse = warehouseFactory.Create(detailFactory.Create());
            Service service = serviceFactory.Create(warehouse, carFactory);

            service.Work();
        }

    }

    class ServiceFactory
    {
        private Queue<Car> _cars = new Queue<Car>();

        public Service Create(Warehouse warehouse, CarFactory carFactory)
        {
            FillCarQueue(carFactory);

            return new Service(warehouse, new Queue<Car>(_cars));
        }

        private void FillCarQueue(CarFactory carFactory)
        {
            int queueCapacity = 10;

            for (int i = 0; i < queueCapacity; i++)
            {
                _cars.Enqueue(carFactory.Create());
            }
        }
    }

    class WarehouseFactory
    {
        private List<Detail> _details = new List<Detail>();

        public Warehouse Create(List<Detail> details)
        {
            FillComponents(details);

            return new Warehouse(_details);
        }

        private void FillComponents(List<Detail> details)
        {
            int detailsCapacity = 30;

            for (int i = 0; i < detailsCapacity; i++)
            {
                _details.Add(details[UserUtils.GenerateRandomNumber(details.Count)]);
            }
        }
    }

    class CarFactory
    {
        private List<Detail> _details = new List<Detail>();

        public CarFactory(List<Detail> details)
        {
            _details = new List<Detail>(details);
        }

        public Car Create()
        {
            List<Detail> tempDetails = new List<Detail>();

            for (int i = 0; i < _details.Count; i++)
            {
                tempDetails.Add(new Detail(_details[i].Name, TryGetBrokenStatus()));
            }

            return new Car(tempDetails, GetName());
        }

        private string GetName()
        {
            List<string> names = new List<string>
            {
                "BMW",
                "VolksWagen",
                "Audi",
                "TANK",
                "Lexus",
                "Honda",
                "Chevrolet"
            };

            return names[UserUtils.GenerateRandomNumber(names.Count)];
        }

        private bool TryGetBrokenStatus()
        {
            int breakChance = 2;

            return (UserUtils.GenerateRandomNumber(breakChance + 1) == breakChance);
        }
    }

    class DetailFactory
    {
        private List<Detail> _details = new List<Detail>();

        public DetailFactory()
        {
            FillList();
        }

        public List<Detail> Create()
        {
            return new List<Detail>(_details);
        }

        private void FillList()
        {
            _details.Add(new Detail("Тормозные колодки"));
            _details.Add(new Detail("Тормозные диски"));
            _details.Add(new Detail("Бампер"));
            _details.Add(new Detail("Топливный насос"));
            _details.Add(new Detail("Глушитель"));
            _details.Add(new Detail("Стекло"));
            _details.Add(new Detail("Электроника"));
            _details.Add(new Detail("Трансмиссия"));
            _details.Add(new Detail("Поршень"));
            _details.Add(new Detail("Аккумулятор"));
            _details.Add(new Detail("Форсунки"));
        }
    }

    class Service
    {
        private int _money = 10000;
        private Queue<Car> _cars = new Queue<Car>();
        private Warehouse _warehouse;

        public Service(Warehouse warehouse, Queue<Car> cars)
        {
            _warehouse = warehouse;
            _cars = new Queue<Car>(cars);
        }

        public void Work()
        {
            const string CommandStartRepair = "1";
            const string CommandShowQueue = "2";
            const string CommandShowDetails = "3";
            const string CommandExit = "4";

            bool isWork = true;

            while (isWork)
            {
                Console.WriteLine($"!АВТОСЕРВИС! Баланс: {_money}");
                Console.WriteLine("Меню: ");
                Console.WriteLine($"{CommandStartRepair} - Начать ремонт " +
                                  $"\n{CommandShowQueue} - Показать очередь " +
                                  $"\n{CommandShowDetails} - Показать запчасти на складе " +
                                  $"\n{CommandExit} - Выйти ");
                Console.Write("Выберите нужный пункт: ");

                switch (Console.ReadLine())
                {
                    case CommandStartRepair:
                        Repair();
                        break;

                    case CommandShowQueue:
                        ShowQueue();
                        break;

                    case CommandShowDetails:
                        _warehouse.ShowDetails();
                        break;

                    case CommandExit:
                        isWork = false;
                        break;

                    default:
                        Console.WriteLine("Такого пункта нет!");
                        break;
                }

                Console.Write("\nНажмите клавишу для продолжения");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private void Repair()
        {
            const string CommandContinue = "1";
            const string CommandStopRepair = "2";

            bool isWork = true;

            Car car = _cars.Dequeue();

            Queue<Detail> brokenDetails = car.GetBrokenDetail();
            List<Detail> repairedDetails = new List<Detail>();
            List<Detail> unrepairedDetail = new List<Detail>();

            while (isWork && brokenDetails.Count != 0)
            {
                Console.Clear();
                Console.WriteLine($"!Ремонт авто {car.Name}!");

                ShowBrokenDetails(car);

                Console.WriteLine("\nМеню: ");
                Console.WriteLine($"{CommandContinue} - Ремонтировать " +
                                  $"\n{CommandStopRepair} - Закончить ремонт досрочно");
                Console.Write("Выберите нужный пункт: ");

                switch (Console.ReadLine())
                {
                    case CommandContinue:
                        ChangeDetail(brokenDetails.Dequeue(), car, repairedDetails, unrepairedDetail);
                        break;

                    case CommandStopRepair:
                        isWork = false;
                        break;

                    default:
                        Console.WriteLine("Такого пункта нет!");
                        break;
                }

                Console.Write("\nНажмите клавишу для продолжения");
                Console.ReadKey();
            }

            CalculateResult(brokenDetails, repairedDetails, unrepairedDetail);
        }

        private void ChangeDetail(Detail brokenComponent, Car car,
            List<Detail> repairedComponents, List<Detail> unRepaireComponent)
        {
            if (_warehouse.TryGetDetail(brokenComponent.Name, out Detail component))
            {
                repairedComponents.Add(brokenComponent);
                car.ChangeDetail(component, brokenComponent);
            }
            else
            {
                Console.WriteLine($"\n{brokenComponent.Name} починить не удалось, нет нужной запчасти\n");
                unRepaireComponent.Add(brokenComponent);
            }
        }

        private void ShowQueue()
        {
            Console.WriteLine($"\nВ очереди машины: \n");

            foreach (var car in _cars)
            {
                Console.WriteLine($"{car.Name}");
            }
        }

        private void ShowBrokenDetails(Car car)
        {
            Console.WriteLine("\nСломанные детали: \n");

            foreach (var component in car.GetBrokenDetail())
            {
                Console.WriteLine($"{component.Name}");
            }
        }

        private void CalculateResult(Queue<Detail> brokenDetails,
            List<Detail> repairedDetails, List<Detail> unRepaireDetails)
        {
            int workPrice;
            int repairPrice;
            int divider = 2;
            int fine = 700;

            if (repairedDetails.Count > 0)
            {
                foreach (var detail in repairedDetails)
                {
                    workPrice = detail.Price / divider;
                    repairPrice = detail.Price + workPrice;
                    _money += repairPrice;

                    Console.WriteLine($"\n+ {repairPrice}, за ремонт {detail.Name}");
                }
            }

            if (brokenDetails.Count > 0 || unRepaireDetails.Count > 0)
            {
                int currentFine = brokenDetails.Count * fine + unRepaireDetails.Count * fine;

                _money -= currentFine;

                Console.WriteLine($"\nЗа непочиненные детали вычтен штраф {currentFine}");
            }
        }
    }

    class Warehouse
    {
        private List<Detail> _details = new List<Detail>();

        public Warehouse(List<Detail> details)
        {
            _details = new List<Detail>(details);
        }

        public bool TryGetDetail(string name, out Detail detail)
        {
            detail = null;

            for (int i = 0; i < _details.Count; i++)
            {
                if (_details[i].Name == name)
                {
                    detail = _details[i];
                    _details.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void ShowDetails()
        {
            Console.WriteLine($"\nДетали в наличии\n");

            foreach (var detail in _details)
            {
                Console.WriteLine($"{detail.Name}");
            }
        }
    }

    class Car
    {
        private List<Detail> _details = new List<Detail>();

        public Car(List<Detail> details, string name)
        {
            _details = new List<Detail>(details);
            Name = name;
        }

        public string Name { get; private set; }

        public void ChangeDetail(Detail detail, Detail brokenDetail)
        {
            _details.Remove(brokenDetail);
            _details.Add(detail);
        }

        public Queue<Detail> GetBrokenDetail()
        {
            Queue<Detail> brokenDetails = new Queue<Detail>();

            for (int i = 0; i < _details.Count; i++)
            {
                if (_details[i].IsBroken)
                {
                    brokenDetails.Enqueue(_details[i]);
                }
            }

            return brokenDetails;
        }
    }

    class Detail
    {
        public Detail(string name, bool isBroken = false)
        {
            Name = name;
            Price = GeneratePrice();
            IsBroken = isBroken;
        }

        public string Name { get; private set; }
        public int Price { get; private set; }
        public bool IsBroken { get; private set; }

        private int GeneratePrice()
        {
            int minPrice = 1000;
            int maxPrice = 10000;

            return UserUtils.GenerateRandomNumber(minPrice, maxPrice + 1);
        }
    }

    class UserUtils
    {
        private static Random s_random = new Random();

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            return s_random.Next(minValue, maxValue);
        }

        public static int GenerateRandomNumber(int maxValue)
        {
            return s_random.Next(maxValue);
        }
    }
}
