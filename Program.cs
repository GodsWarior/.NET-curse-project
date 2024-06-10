using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


class Program
{

    //38 методов + main
    static void Main(string[] args)
    {
        (string filePathClients, string filePathProducts, string filePathOrders) = getFilePaths();
        (Client[] clients, Product[] products, Order[] orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
        bool exit = false;
        while (!exit)
        {
            switch (choiceMainMenu())
            {
                case "0":
                    exit = true;
                    break;

                case "1":
                    beauty(() => View(clients, products, orders), false);
                    break;

                case "2":
                    beauty(() => Create(filePathClients, filePathProducts, filePathOrders), false);
                    (clients, products, orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
                    break;

                case "3":
                    beauty(() => Edit(filePathClients, filePathProducts, filePathOrders), false);
                    (clients, products, orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
                    break;

                case "4":
                    beauty(()=> Delete(filePathClients, filePathProducts, filePathOrders), false);
                    (clients, products, orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
                    break;

                case "5":
                    beauty(() => Find(filePathClients, filePathProducts, filePathOrders), false);
                    break;


                default:
                    beauty(() => Console.WriteLine("Неверный ввод. Пожалуйста, выберите существующий пункт меню."));
                    break;
            }
        }
    }

    public struct Client
    {
        [JsonPropertyName("id")] public int id { get; set; }
        [JsonPropertyName("name")] public string name { get; set; }
        [JsonPropertyName("email")] public string email { get; set; }
        [JsonPropertyName("phone")] public string phone { get; set; }
    }

    public struct Product
    {
        [JsonPropertyName("id")] public int id { get; set; }
        [JsonPropertyName("name")] public string name { get; set; }
        [JsonPropertyName("pricePerMounth")] public double pricePerMounth { get; set; }
        [JsonPropertyName("amount")] public int amount { get; set; }
    }

    public struct Order
    {
        [JsonPropertyName("id")] public int id { get; set; }
        [JsonPropertyName("idClient")] public int idClient { get; set; }
        [JsonPropertyName("idProduct")] public int idProduct { get; set; }
        [JsonPropertyName("date")] public string date { get; set; }
        [JsonPropertyName("rentalTime")] public int rentalTime { get; set; }
    }

    public static T[] getData<T>(string Path)
    {
        if (File.Exists(Path))
        {
            string jsonString = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<T[]>(jsonString);
        }
        else
        {
            Console.WriteLine("Файл не найден");
        }
        return null;
    }

    public static string choiceMainMenu()
    {
        Console.Clear();
        Console.WriteLine("0. Выйти");
        Console.WriteLine("1. Просмотр ");
        Console.WriteLine("2. Создание ");
        Console.WriteLine("3. Редактирование ");
        Console.WriteLine("4. Удаление ");
        Console.WriteLine("5. Поиск заказов ");
        Console.Write("Выберите действие: ");
        string choice = Console.ReadLine();
        return choice;
    }

    public static string choiceFindMenu()
    {
        Console.Clear();
        Console.WriteLine("0. Выйти");
        Console.WriteLine("1. Поиск по клиенту ");
        Console.WriteLine("2. Поиск по продукту ");
        Console.WriteLine("3. Поиск по периоду ");
        Console.Write("Выберите действие: ");
        string choice = Console.ReadLine();
        return choice;
    }

    public static Order[] findOrdersForClients(Client[] clients, Product[] products, Order[] orders, int id)
    {
        Order[] findOrders = new Order[0];
        foreach (var order in orders)
        {
            if (order.idClient == id)
            {
                Array.Resize(ref findOrders, findOrders.Length + 1);
                findOrders[findOrders.Length - 1] = order;
            }
        }
        return findOrders;
    }

    public static Order[] findOrdersForProducts(Client[] clients, Product[] products, Order[] orders, int id)
    {
        Order[] findOrders = Array.FindAll(orders, o => o.idProduct == id);
        return findOrders;
    }

    public static Order[] findOrdersForPeriod(Client[] clients, Product[] products, Order[] orders, string[] period)
    {
        DateTime startDate = DateTime.Parse(period[0]);
        DateTime endDate = DateTime.Parse(period[1]);
        Order[] findOrders = Array.FindAll(orders, o =>
        {
            DateTime orderDate = DateTime.Parse(o.date);
            return orderDate >= startDate && orderDate <= endDate;
        });
        return findOrders;
    }

    public static int choiceIdClient(Client[] clients)
    {
        Console.Clear();
        showClients(clients);
        Console.Write("Выберите id клиента, по которому хотите найти заказы: ");
        string input = Console.ReadLine();
        if (!int.TryParse(input, out int id))
        {
            return -1;
        }
        bool clientExists = false;
        foreach (var client in clients)
        {
            if (client.id == id)
            {
                clientExists = true;
                break;
            }
        }
        if (!clientExists)
        {
            return -1;
        }
        return id;
    }

    public static int choiceIdProduct(Product[] products)
    {
        Console.Clear();
        showProducts(products);
        Console.Write("Выберите id продукта, по которому хотите найти заказы: ");
        string input = Console.ReadLine();
        if (!int.TryParse(input, out int id))
        {
            return -1;
        }
        bool productExists = false;
        foreach (var product in products)
        {
            if (product.id == id)
            {
                productExists = true;
                break;
            }
        }
        if (!productExists)
        {
            return -1;
        }
        return id;
    }

    public static string[] choicePeriod()
    {
        Console.Write("Введите период за который хотите найти заказ в формате 20xx-xx-xx/20xx-xx-xx: ");
        string[] period = Console.ReadLine().Replace(" ", "").Split('/');
        if (period.Length != 2 || !DateTime.TryParse(period[0], out DateTime startDate) || !DateTime.TryParse(period[1], out DateTime endDate))
        {
            return null;
        }
        return period;
    }

    public static void menuFindCaseOne(Client[] clients, Product[] products, Order[] orders)
    {
        int id = choiceIdClient(clients);
        if (id != -1)
        {
            Order[] findOrders = findOrdersForClients(clients, products, orders, id);
            if (findOrders.Length != 0)
                beauty(() => showOrders(findOrders, clients, products));
            else
                beauty(() => Console.WriteLine("У данного клиента нету заказов"));
        }
        else
            beauty(() => Console.WriteLine("Неверный формат id"));
    }

    public static void menuFindCaseTwo(Client[] clients, Product[] products, Order[] orders)
    {
        int id = choiceIdProduct(products);
        if (id != -1)
        {
            Order[] findOrders = findOrdersForProducts(clients, products, orders, id);
            if (findOrders.Length != 0)
                beauty(() => showOrders(findOrders, clients, products));
            else
                beauty(() => Console.WriteLine("На данный продукт нету заказов"));
        }
        else
            beauty(() => Console.WriteLine("Неверный формат id"));
    }

    public static void menuFindCaseThree(Client[] clients, Product[] products, Order[] orders)
    {
        Console.Clear();
        string[] period = choicePeriod();
        if (period == null)
        {
            beauty(() => Console.WriteLine("Неправильный формат ввода. Ожидается две даты, разделенные слэшем в формате 20xx-xx-xx.(год, месяц, день)"));
            return;
        }
        Order[] findOrders = findOrdersForPeriod(clients, products, orders, period);
        beauty(() => showOrders(findOrders, clients, products));
    }

    public static void Find(string filePathClients, string filePathProducts, string filePathOrders)
    {
        (Client[] clients, Product[] products, Order[] orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
        bool run = true;
        while (run)
        {

            switch (choiceFindMenu())
            {
                case "0":
                    run = false;
                    break;

                case "1":
                    menuFindCaseOne(clients, products, orders);
                    //поиск по клиенту
                    break;

                case "2":
                    menuFindCaseTwo(clients, products, orders);
                    //поиск по продукту
                    break;

                case "3":
                    menuFindCaseThree(clients, products, orders);
                    //поиск по периоду
                    break;

                default:
                    beauty(() => Console.WriteLine("Неверный ввод. Пожалуйста, выберите существующий пункт меню."));
                    break;

            }
        }

    }

    private bool IsNumeric(string value)
    {
        return int.TryParse(value, out _);
    }

    public static void showClients(Client[] clients, string options = "id")
    {
        if (options == "id")
            Array.Sort(clients, (c1, c2) => c1.id - c2.id);
        else if (options == "alphabet")
            Array.Sort(clients, (с1, с2) => string.Compare(с1.name, с2.name));
        Console.WriteLine("{0,-5}\t{1,-20}\t{2,-15}\t{3}", "ID", "Имя Фамилия", "Моб. телефон", "Email");
        foreach (var client in clients)
        {
            Console.WriteLine("{0,-5}\t{1,-20}\t{2,-15}\t{3}", client.id, client.name, client.phone, client.email);
        }
    }

    public static void showOrders(Order[] orders, Client[] clients, Product[] products, string options = "id")
    {
        if (options == "id")
            Array.Sort(orders, (c1, c2) => c1.id - c2.id);
        else if (options == "date")
            Array.Sort(orders, (x, y) => DateTime.Parse(x.date).CompareTo(DateTime.Parse(y.date)));
        else if(options == "deadline")
        {
            Array.Sort(orders, (x, y) => {
                if (string.IsNullOrEmpty(x.date) && string.IsNullOrEmpty(y.date)) return 0;
                if (string.IsNullOrEmpty(x.date)) return -1;
                if (string.IsNullOrEmpty(y.date)) return 1;
                DateTime openDateX = DateTime.ParseExact(x.date, "yyyy-MM-dd", null);
                DateTime closeDateX = openDateX.AddDays(x.rentalTime);
                DateTime openDateY = DateTime.ParseExact(y.date, "yyyy-MM-dd", null);
                DateTime closeDateY = openDateY.AddDays(y.rentalTime);
                return closeDateX.CompareTo(closeDateY);
            });
        }


        Console.WriteLine("{0,-10}\t{1,-20}\t{2,-20}\t{3,-20}\t{4,-20}", "ID заказа", "Клиент", "Продукт", "Дата заказа", "Время аренды");
        foreach (var order in orders)
        {
            string clientName = Array.Find(clients, client => client.id == order.idClient).name;
            string productName = Array.Find(products, product => product.id == order.idProduct).name;
            Console.WriteLine("{0,-10}\t{1,-20}\t{2,-20}\t{3,-20}\t{4,-20}", order.id, clientName, productName, order.date, order.rentalTime);
        }
    }

    public static void showProducts(Product[] products, string options = "id")
    {
        if (options == "id")
            Array.Sort(products, (c1, c2) => c1.id - c2.id);
        else if (options == "alphabet")
            Array.Sort(products, (с1, с2) => string.Compare(с1.name, с2.name));
        Console.WriteLine("{0,-5}\t{1,-20}\t{2,-15}\t{3}", "ID", "Название", "Количество", "Цена за месяц");
        foreach (var product in products)
        {
            Console.WriteLine("{0,-5}\t{1,-20}\t{2,-15}\t{3}", product.id, product.name, product.amount, product.pricePerMounth + "$");
        }
    }

    public static (Client[], Product[], Order[]) getArrays(string filePathClients, string filePathProducts, string filePathOrders)
    {
        return (getData<Client>(filePathClients), getData<Product>(filePathProducts), getData<Order>(filePathOrders));
    }

    public static void beauty(Action action, bool isNeedReadLine = true)
    {
        Console.Clear();
        action.Invoke();
        if (isNeedReadLine)
            Console.ReadLine();
    }

    public static string viewMenuChoice()
    {
        Console.Clear();
        Console.WriteLine("0. Выйти");
        Console.WriteLine("1. Просмотреть список клиентов");
        Console.WriteLine("2. Просмотреть товары");
        Console.WriteLine("3. Просмотреть заказы");
        Console.Write("Выберите действие: ");
        string SearchChoice = Console.ReadLine();
        return SearchChoice;
    }

    public static string createMenuChoice()
    {
        Console.Clear();
        Console.WriteLine("0. Выход");
        Console.WriteLine("1. Добавить клиента");
        Console.WriteLine("2. Добавить продукт");
        Console.WriteLine("3. Добавить заказ");
        Console.Write("Выберите действие: ");
        string SearchChoice = Console.ReadLine();
        return SearchChoice;
    }

    public static string editMenuChoice()
    {
        Console.Clear();
        Console.WriteLine("0. Выход");
        Console.WriteLine("1. Редактировать клиента");
        Console.WriteLine("2. Редактировать товар");
        Console.WriteLine("3. Редактировать заказ");
        Console.Write("Выберите действие: ");
        string SearchChoice = Console.ReadLine();
        return SearchChoice;
    }

    public static string deleteMenuChoice()
    {
        Console.Clear();
        Console.WriteLine("0. Выйти");
        Console.WriteLine("1. Удалить клиента ");
        Console.WriteLine("2. Удалить продукт ");
        Console.WriteLine("3. Удалить заказ ");
        Console.Write("Выберите действие: ");
        string SearchChoice = Console.ReadLine();
        return SearchChoice;
    }

    public static void viewClients(Client[] clients)
    {
        beauty(() => showClients(clients), false);
        Console.Write("Хотите отсортировать вывод?(Y-да, enter - выйти) ");
        string answer = Console.ReadLine();
        if (answer.ToLower() == "y")
        {
            Console.Clear();
            Console.WriteLine("1 - сортировка по id");
            Console.WriteLine("2 - сортировка по алфавиту");
            string SearchChoice = Console.ReadLine();
            switch (SearchChoice)
            {
                case "1":
                    beauty(() => showClients(clients, "id"));
                    break;
                case "2":
                    beauty(() => showClients(clients, "alphabet"));
                    break;
                default:
                    break;
            }
        }
    }

    public static void viewProducts(Product[] products)
    {
        beauty(() => showProducts(products), false);
        Console.Write("Хотите отсортировать вывод?(Y-да, enter - выйти) ");
        string answer = Console.ReadLine();
        if (answer.ToLower() == "y")
        {
            Console.Clear();
            Console.WriteLine("1 - сортировка по id");
            Console.WriteLine("2 - сортировка по алфавиту");
            string SearchChoice = Console.ReadLine();
            switch (SearchChoice)
            {
                case "1":
                    beauty(() => showProducts(products, "id"));
                    break;
                case "2":
                    beauty(() => showProducts(products, "alphabet"));
                    break;
                default:
                    break;
            }
        }
    }

    public static void viewOrders(Order[] orders, Client[] clients, Product[] products)
    {
        beauty(() => showOrders(orders, clients, products), false);
        Console.Write("хотите отсортировать вывод?(Y-да, enter - выйти) ");
        string answer = Console.ReadLine();
        if (answer.ToLower() == "y")
        {
            Console.Clear();
            Console.WriteLine("1 - сортировка по id");
            Console.WriteLine("2 - сортировка по дате открытия заказа");
            Console.WriteLine("3 - сортировка по приблежению закрытия заказа");
            string SearchChoice = Console.ReadLine();
            switch (SearchChoice)
            {
                case "1":
                    beauty(() => showOrders(orders, clients, products, "id"));
                    break;
                case "2":
                    beauty(() => showOrders(orders, clients, products, "date"));
                    break;
                case "3":
                    beauty(() => showOrders(orders, clients, products, "deadline"));
                    break;
                default:
                    break;
            }
        }
    }

    public static void View(Client[] clients, Product[] products, Order[] orders)
    {
        bool Run = true;
        while (Run)
        {
            switch (viewMenuChoice())
            {
                case "0":
                    Run = false;
                    break;

                case "1":
                    viewClients(clients);
                    break;

                case "2":
                    viewProducts(products);
                    break;
                case "3":
                    viewOrders(orders, clients, products);
                    break;

                default:
                    beauty(() => Console.WriteLine("Неверный ввод"));
                    break;
            }
        }
    }

    public static string clientValidation(Client client)
    {
        string name = client.name;
        string email = client.email;
        string phone = client.phone;
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
        {
            return "Ошибка: все поля должны быть заполнены.";
        }
        if (!email.Contains("@") || !email.Contains("."))
        {
            return "Ошибка: неправильный формат email.";
        }
        if (phone.Length != 13 || !Array.TrueForAll(phone.Substring(1).ToCharArray(), s => char.IsDigit(s)))
        {
            return "Ошибка: неправильный формат телефона.";
        }
        return "";
    }

    public static Client createClient(string filePathClients)
    {
        Client[] clients = getData<Client>(filePathClients);
        Console.Write("Введите имя и фамилию клиента: ");
        string name = Console.ReadLine();
        Console.Write("Введите email клиента: ");
        string email = Console.ReadLine();
        Console.Write("Введите телефон клиента: ");
        string phone = Console.ReadLine();
        Client clientWithMaxId = clients[0];
        foreach (Client client in clients)
        {
            if (client.id > clientWithMaxId.id)
            {
                clientWithMaxId = client;
            }
        }
        Client newClient = new Client { id = clientWithMaxId.id + 1, name = name, email = email, phone = phone };
        return newClient;
    }

    public static Product? createProduct(string filePathProducts)
    {
        Product[] products = getData<Product>(filePathProducts);
        Console.Write("Введите название продукта: ");
        string name = Console.ReadLine();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        Console.Write("Введите цену за месяц аренды: ");
        string pricePerMonthInput = Console.ReadLine();
        if (!double.TryParse(pricePerMonthInput, out double pricePerMonth))
        {
            return null;
        }
        Console.Write("Введите количество этого продукта: ");
        string amountInput = Console.ReadLine();
        if (!int.TryParse(amountInput, out int amount))
        {
            return null;
        }
        Product productWithMaxId = products[0];
        for (int i = 1; i < products.Length; i++)
        {
            if (products[i].id > productWithMaxId.id)
            {
                productWithMaxId = products[i];
            }
        }
        Product newProduct = new Product
        {
            id = productWithMaxId.id + 1,
            name = name,
            pricePerMounth = pricePerMonth,
            amount = amount
        };
        return newProduct;
    }

    public static Order? createOrder(string filePathOrders, string filePathClients, string filePathProducts)
    {
        (Client[] clients, Product[] products, Order[] orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
        showClients(clients);
        Console.Write("Выберите id клиента, на которого заказ ");
        if (!int.TryParse(Console.ReadLine(), out int idClient) || Array.FindIndex(clients, c => c.id == idClient) == -1)
        {
            return null;
        }
        beauty(() => showProducts(products), false);
        Console.Write("Выберите id продукта, который берёт клиент ");
        if (!int.TryParse(Console.ReadLine(), out int idProduct) || Array.FindIndex(products, p => p.id == idProduct) == -1)
        {
            return null;
        }
        beauty(() => Console.Write("Введите сегодняшнюю дату (20xx-xx-xx) "), false);
        string date = Console.ReadLine().Replace(" ", "");
        if (!DateTime.TryParse(date, out _))
        {
            return null;
        }
        Console.Write("Введите срок аренды(в днях) ");
        if (!int.TryParse(Console.ReadLine(), out int rentalTime))
        {
            return null;
        }
        Order orderWithMaxId = orders[0];
        for (int i = 1; i < orders.Length; i++)
        {
            if (orders[i].id > orderWithMaxId.id)
            {
                orderWithMaxId = orders[i];
            }
        }
        Order newOrder = new Order { id = orderWithMaxId.id + 1, idClient = idClient, idProduct = idProduct, date = date, rentalTime = rentalTime };
        return newOrder;
    }

    public static void addClient(string filePathClients)
    {
        Client newClient = createClient(filePathClients);
        string validation = clientValidation(newClient);
        if (validation != "")
        {
            beauty(() => Console.WriteLine(validation));
            return;
        }
        add<Client>(filePathClients, newClient);
        Console.WriteLine("Запись добавлена");
    }

    public static void addProduct(string filePathProducts)
    {
        Product? newProduct = createProduct(filePathProducts);
        if (newProduct != null)
        {
            add(filePathProducts, newProduct);
            Console.WriteLine("Запись добавлена");
        }
        else
        {
            beauty(() => Console.WriteLine("Неверный ввод"), false);
            return;
        }
    }

    public static void addOrder(string filePathOrders, string filePathClients, string filePathProducts)
    {
        Order? newOrder = createOrder(filePathOrders, filePathClients, filePathProducts);
        if (newOrder != null)
        {
            add(filePathOrders, newOrder);
            Console.WriteLine("Запись добавлена");
        }
        else
        {
            beauty(() => Console.WriteLine("Неверный ввод"), false);
        }
    }

    public static void Create(string filePathClients, string filePathProducts, string filePathOrders)
    {
        (Client[] clients, Product[] products, Order[] orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
        bool Run = true;
        while (Run)
        {
            switch (createMenuChoice())
            {
                case "0":
                    Run = false;
                    break;
                case "1":
                    beauty(() => addClient(filePathClients));
                    clients = getData<Client>(filePathClients);
                    break;
                case "2":
                    beauty(() => addProduct(filePathProducts));
                    products = getData<Product>(filePathProducts);
                    break;
                case "3":
                    beauty(() => addOrder(filePathOrders, filePathClients, filePathProducts));
                    orders = getData<Order>(filePathOrders);
                    break;
                default:
                    beauty(() => Console.WriteLine("невенрный ввод"));
                    break;
            }
        }
    }

    public static void Edit(string filePathClients, string filePathProducts, string filePathOrders)
    {
        bool run = true;
        while (run)
        {
            switch (editMenuChoice())
            {
                case "0":
                    run = false;
                    break;
                case "1":
                    beauty(() => editingClient(filePathClients));
                    break;
                case "2":
                    beauty(() => editingProduct(filePathProducts));
                    break;
                case "3":
                    beauty(() => editingOrder(filePathOrders, filePathClients, filePathProducts));
                    break;
                default:
                    beauty(() => Console.WriteLine("Неверный ввод"));
                    break;
            }
        }
    }

    public static void edit<Type>(string path, Type entity) where Type : struct
    {
        string jsonString = File.ReadAllText(path);
        Type[] entities = JsonSerializer.Deserialize<Type[]>(jsonString);
        int index = findIndexById(entities, entity);
        if (index != -1)
        {
            entities[index] = entity;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string jsonStr = JsonSerializer.Serialize(entities, options);
            File.WriteAllText(path, jsonStr);
        }
    }

    public static Client? createClientForEdit(string filePathClients)
    {
        Client[] clients = getData<Client>(filePathClients);
        showClients(clients);
        Console.Write("Выберите id клиента, которого хотите поменять: ");
        string inputId = Console.ReadLine();
        if (!int.TryParse(inputId, out int id) || Array.FindIndex(clients, c => c.id == int.Parse(inputId)) == -1)
        {
            return null;
        }
        Client client = Array.Find(clients, c => c.id == id);
        Console.Write("Введите новое имя (или нажмите Enter, чтобы оставить без изменений): ");
        string nameInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(nameInput))
            client.name = nameInput;
        Console.Write("Введите новый телефон (или нажмите Enter, чтобы оставить без изменений): ");
        string phone = Console.ReadLine();
        if (!string.IsNullOrEmpty(phone))
        {
            if (phone.Length != 13 || !Array.TrueForAll(phone.Substring(1).ToCharArray(), s => char.IsDigit(s)))
            {
                return null;
            }
            client.phone = phone;
        }

        Console.Write("Введите новый email (или нажмите Enter, чтобы оставить без изменений): ");
        string email = Console.ReadLine();
        if (!string.IsNullOrEmpty(email))
        {
            if (!email.Contains("@") || !email.Contains("."))
            {
                return null;
            }
            client.email = email;
        }
        return client;
    }

    public static Product? createProductForEdit(string filePathProducts)
    {
        Product[] products = getData<Product>(filePathProducts);
        showProducts(products);
        Console.Write("Выберите id продукта, который хотите поменять: ");
        string inputId = Console.ReadLine();
        if (!int.TryParse(inputId, out int id) || Array.FindIndex(products, p => p.id == int.Parse(inputId)) == -1)
        {
            return null;
        }
        Product product = Array.Find(products, p => p.id == id);
        Console.Write("Введите новое название (или нажмите Enter, чтобы оставить без изменений): ");
        string nameInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(nameInput))
            product.name = nameInput;
        Console.Write("Введите новую цену за месяц аренды (или нажмите Enter, чтобы оставить без изменений): ");
        string pricePerMounthInput = Console.ReadLine();
        if (int.TryParse(pricePerMounthInput, out int pricePerMounth))
            product.pricePerMounth = pricePerMounth;
        Console.Write("Введите новое кол-во товара (или нажмите Enter, чтобы оставить без изменений): ");
        string amountInput = Console.ReadLine();
        if (int.TryParse(amountInput, out int amount))
            product.amount = amount;
        return product;
    }

    public static Order? createOrderForEdit(string filePathOrders, string filePathClients, string filePathProducts)
    {
        Client[] clients = getData<Client>(filePathClients);
        Product[] products = getData<Product>(filePathProducts);
        Order[] orders = getData<Order>(filePathOrders);
        showOrders(orders, clients, products);
        Console.Write("Выберите id заказа, который хотите поменять: ");
        string inputId = Console.ReadLine();
        if (!int.TryParse(inputId, out int id) || Array.FindIndex(orders, o => o.id == int.Parse(inputId)) == -1)
        {
            return null;
        }
        Order order = Array.Find(orders, o => o.id == id);
        Console.Clear();
        showClients(getData<Client>(filePathClients));
        Console.Write("Выберите id клиента, на которого хотите переоформить заказ (или нажмите Enter, чтобы оставить без изменений):");
        string idClientInput = Console.ReadLine();
        if (int.TryParse(idClientInput, out int idCLient) && Array.FindIndex(clients, c => c.id == int.Parse(idClientInput)) != -1)
            order.idClient = idCLient;
        Console.Clear();
        showProducts(getData<Product>(filePathProducts));
        Console.Write("Выберите id продукта, на которого хотите заменить текущий (или нажмите Enter, чтобы оставить без изменений):");
        string idProductInput = Console.ReadLine();
        if (int.TryParse(idProductInput, out int idProduct) && Array.FindIndex(products, p => p.id == int.Parse(idProductInput)) != -1)
            order.idProduct = idProduct;
        Console.Clear();
        Console.Write("Введите новую дату 20xx-xx-xx (или нажмите Enter, чтобы оставить без изменений): ");
        string dateInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(dateInput) && DateTime.TryParse(dateInput, out _))
            order.date = dateInput;
        Console.Clear();
        Console.Write("Введите новый срок аренды, считая от введённой даты (или нажмите Enter, чтобы оставить без изменений): ");
        string rentalTimeInput = Console.ReadLine();
        if (int.TryParse(rentalTimeInput, out int rentalTime) && rentalTime > 0)
            order.rentalTime = rentalTime;
        return order;
    }

    public static void editingClient(string filePathClients)
    {
        Client? newClient = createClientForEdit(filePathClients);
        if (newClient != null)
        {
            Console.Clear();
            edit<Client>(filePathClients, newClient.Value);
            Client[] clients = getData<Client>(filePathClients);
            showClients(clients);
        }
        else
            beauty(() => Console.WriteLine("Неверный ввод"), false);
    }

    public static void editingProduct(string filePathProducts)
    {
        Product? newProduct = createProductForEdit(filePathProducts);
        if (newProduct != null)
        {
            edit(filePathProducts, newProduct.Value);
            Product[] products = getData<Product>(filePathProducts);
            beauty(() => showProducts(products), false);
        }
        else
        {
            Console.WriteLine("Неверный ввод");
        }
    }

    public static void editingOrder(string filePathOrders, string filePathClients, string filePathProducts)
    {
        Order? newOrder = createOrderForEdit(filePathOrders, filePathClients, filePathProducts);
        Console.Clear();
        edit(filePathOrders, newOrder.Value);
        Order[] orders = getData<Order>(filePathOrders);
        Client[] clients = getData<Client>(filePathClients);
        Product[] products = getData<Product>(filePathProducts);
        showOrders(orders, clients, products);
    }

    public static void delClient(string filePathOrders, string filePathClients, Client delClient)
    {
        del<Client>(delClient, filePathOrders, filePathClients);
        Client[] clients = getData<Client>(filePathClients);
        Console.Clear();
        showClients(clients);
    }

    public static void delProduct(string filePathOrders, string filePathProducts, Product delProduct)
    {
        del<Product>(delProduct, filePathOrders, filePathProducts);
        Order[] orders = getData<Order>(filePathOrders);
        Product[] products = getData<Product>(filePathProducts);
        beauty(()=>showProducts(products), false);
    }

    public static void delOrder(string filePathOrders, string filePathClients, string filePathProducts, Order delOrder)
    {
        Client[] clients = getData<Client>(filePathClients);
        Product[] products = getData<Product>(filePathProducts);
        del<Order>(delOrder, filePathOrders);
        Order[] orders = getData<Order>(filePathOrders);
        beauty(()=>showOrders(orders, clients, products), false);
    }

    public static void deleteMenuCaseOne(string filePathOrders, string filePathClients)
    {
        Order[] orders = getData<Order>(filePathOrders);
        Client[] clients = getData<Client>(filePathClients);
        showClients(clients);
        Console.Write("Выберите id клиента, которого хотите удалить: ");
        string inputId = Console.ReadLine();
        if (!int.TryParse(inputId, out int id) || Array.FindIndex(clients, c => c.id == int.Parse(inputId)) == -1)
        {
            beauty(() => Console.WriteLine("Некорректный выбор id"), false);
            return;
        }
        Client client = Array.Find(clients, c => c.id == id);
        beauty(() => delClient(filePathOrders, filePathClients, client), false);
    }

    public static void deleteMenuCaseTwo(string filePathOrders, string filePathProducts)
    {
        Order[] orders = getData<Order>(filePathOrders);
        Product[] products = getData<Product>(filePathProducts);
        showProducts(products);
        Console.Write("Выберите id продукта, который хотите удалить: ");
        string inputId = Console.ReadLine();
        if (!int.TryParse(inputId, out int id) || Array.FindIndex(products, p => p.id == int.Parse(inputId)) == -1)
        {
            beauty(() => Console.WriteLine("Некорректный выбор id"), false);
            return;
        }
        Product product = Array.Find(products, p => p.id == id);
        beauty(() => delProduct(filePathOrders, filePathProducts, product), false);
    }

    public static void deleteMenuCaseThree(string filePathOrders, string filePathClients, string filePathProducts)
    {
        (Client[] clients, Product[] products, Order[] orders) = getArrays(filePathClients, filePathProducts, filePathOrders);
        showOrders(orders, clients, products);
        Console.Write("Выберите id заказа, который хотите удалить: ");
        string inputId = Console.ReadLine();
        if (!int.TryParse(inputId, out int id) || Array.FindIndex(orders, o => o.id == int.Parse(inputId)) == -1)
        {
            beauty(() => Console.WriteLine("Некорректный выбор id"), false);
            return;
        }
        Order order = Array.Find(orders, o => o.id == id);
        beauty(() => delOrder(filePathOrders, filePathClients, filePathProducts, order), false);
    }

    public static void Delete(string filePathClients, string filePathProducts, string filePathOrders)
    {
        bool run = true;
        while (run)
        {
            switch (deleteMenuChoice())
            {
                case "0":
                    run = false;
                    break;
                case "1":
                    beauty(() => deleteMenuCaseOne(filePathOrders, filePathClients));
                    break;
                case "2":
                    beauty(() => deleteMenuCaseTwo(filePathOrders, filePathProducts));
                    break;
                case "3":
                    beauty(() => deleteMenuCaseThree(filePathOrders, filePathClients, filePathProducts));
                    break;
                default:
                    beauty(() => Console.WriteLine("Неверный ввод"));
                    break;
            }
        }
    }

    public static void add<T>(string filePath, T newData)
    {
        string jsonString = File.ReadAllText(filePath);
        T[] dataArray = JsonSerializer.Deserialize<T[]>(jsonString);
        T[] newDataArray = new T[dataArray.Length + 1];
        dataArray.CopyTo(newDataArray, 0);
        newDataArray[dataArray.Length] = newData;
        string updatedJson = JsonSerializer.Serialize(newDataArray, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, updatedJson);
    }

    public static (string, string, string) getFilePaths()
    {
        string basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePathClients = Path.Combine(basePath, "clients.json");
        string filePathProducts = Path.Combine(basePath, "products.json");
        string filePathOrders = Path.Combine(basePath, "orders.json");
        return (filePathClients, filePathProducts, filePathOrders);
    }

    private static int findIndexById<T>(T[] entities, T entity) where T : struct
    {
        var entityIdProp = typeof(T).GetProperty("id");
        var entityIdValue = entityIdProp.GetValue(entity);
        for (int i = 0; i < entities.Length; i++)
        {
            var currentEntityIdValue = entityIdProp.GetValue(entities[i]);
            if (currentEntityIdValue.Equals(entityIdValue))
                return i;
        }
        return -1;
    }

    public static void del<Type>(Type delEntity, string pathOrders, string path = "") where Type : struct
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (!string.IsNullOrEmpty(path))
        {
            Type[] entities = getData<Type>(path);
            int index = findIndexById(entities, delEntity);
            if (index != -1)
            {
                for (int i = index; i < entities.Length - 1; i++)
                    entities[i] = entities[i + 1];
                Array.Resize(ref entities, entities.Length - 1);
                string jsonStr = JsonSerializer.Serialize(entities, options);
                File.WriteAllText(path, jsonStr);
            }
        }

        string propertyName = "id";
        string nameType = typeof(Type).Name;
        Order[] orders = getData<Order>(pathOrders);
        int countToRemove = 0;
        var entityProp = delEntity.GetType().GetProperty("id").GetValue(delEntity);
        foreach (Order order in orders)
        {   
            if ("id" + nameType != "idOrder")
                propertyName = "id" + nameType;
            var orderProp = order.GetType().GetProperty(propertyName).GetValue(order);
            if (orderProp.Equals(entityProp))
                countToRemove++;
        }
        Order[] newOrders = new Order[orders.Length - countToRemove];
        int newIndex = 0;
        foreach (Order order in orders)
        {
            if ("id" + nameType != "idOrder")
                propertyName = "id" + nameType;
            var orderProp = order.GetType().GetProperty(propertyName).GetValue(order);
            if (!orderProp.Equals(entityProp))
            {
                newOrders[newIndex] = order;
                newIndex++;
            }
        }
        string jsonString = JsonSerializer.Serialize(newOrders, options);
        File.WriteAllText(pathOrders, jsonString);
    }

}