namespace PROJECT_SBV1.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string GSTIN { get; set; } = string.Empty;

        public Customer() { }

        public Customer(int customerId, string name, string email, string phone, string address, string city, string country = "India")
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
            Phone = phone;
            Address = address;
            City = city;
            Country = country;
        }

        public Customer(int customerId, string name, string companyName, string email, string phone, string address, string city, string country, string gstin)
        {
            CustomerId = customerId;
            Name = name;
            CompanyName = companyName;
            Email = email;
            Phone = phone;
            Address = address;
            City = city;
            Country = country;
            GSTIN = gstin;
        }
    }
}
