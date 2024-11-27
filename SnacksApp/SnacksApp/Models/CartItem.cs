namespace SnacksApp.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class CartItem : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public decimal TotalValue { get; set; }

        private int _quantity;

        public int Quantity 
        {
            get 
            {
                return _quantity;   
            }
            set 
            {
                if (_quantity != value) 
                { 
                    _quantity = value;
                    OnPropertyChanged();
                }
            } 
        }

        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImagePath => AppConfig.BaseUrl + ImageUrl;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
