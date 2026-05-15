// Tests/ApiIntegrationTests.cs
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfApp1.Tests
{
    [TestClass]
    public class ApiIntegrationTests
    {
        private HttpClient _client;
        private string _baseUrl = "https://localhost:5001/api";
        
        [TestInitialize]
        public void Setup()
        {
            _client = new HttpClient();
        }
        
        [TestMethod]
        public async Task GetProducts_ReturnsOkStatus()
        {
            // Act
            var response = await _client.GetAsync($"{_baseUrl}/products");
            
            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
        
        [TestMethod]
        public async Task GetRecipes_ReturnsOkStatus()
        {
            // Act
            var response = await _client.GetAsync($"{_baseUrl}/recipes");
            
            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
        
        [TestMethod]
        public async Task GetTechMaps_ReturnsOkStatus()
        {
            // Act
            var response = await _client.GetAsync($"{_baseUrl}/techmaps");
            
            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
        
        [TestMethod]
        public async Task PostProduct_ReturnsCreatedStatus()
        {
            // Arrange
            var product = new { name = "Test", productType = "Type", releaseForm = "Form" };
            var content = new StringContent(
                JsonSerializer.Serialize(product),
                Encoding.UTF8,
                "application/json");
            
            // Act
            var response = await _client.PostAsync($"{_baseUrl}/products", content);
            
            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}