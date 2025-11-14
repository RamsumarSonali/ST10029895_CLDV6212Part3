using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ABCRetailers.Models
{
    public class Order : ITableEntity
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }

        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty;

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Customer Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal { get; set; }

        [Display(Name = "Tax")]
        [DataType(DataType.Currency)]
        public decimal Tax { get; set; }

        [Display(Name = "Shipping Cost")]
        [DataType(DataType.Currency)]
        public decimal ShippingCost { get; set; }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Tracking Number")]
        public string? TrackingNumber { get; set; }

        [Display(Name = "Shipped Date")]
        public DateTime? ShippedDate { get; set; }

        [Display(Name = "Delivered Date")]
        public DateTime? DeliveredDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public List<Order> OrderItems { get; set; } = new();

        public Guid OrderItemId { get; set; }
        public Guid OrderID { get; set; }

        [Display(Name = "Product ID")]
        public string ProductId { get; set; } = string.Empty;

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Product Image")]
        public string? ProductImageUrl { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAT { get; set; } = DateTime.UtcNow;



        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        public string CustomerId { get; internal set; }
        public string Username { get; internal set; }
        public DateTimeOffset? Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ETag ETag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string ITableEntity.PartitionKey { get => PartitionKey; set => PartitionKey = value; }
        string ITableEntity.RowKey { get => RowKey; set => RowKey = value; }
    }

    

}