using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Services.AuctionProductsService;

namespace ViewModelLayer.ViewModel
{
    public class CreateAuctionListingViewModel : CreateListingViewModelBase
    {
        public AuctionProductsRepository? AuctionProductsService { get; set; }
        public override void CreateListing(Product product)
        {
            AuctionProductsService?.CreateListing(product);
        }
    }
}
