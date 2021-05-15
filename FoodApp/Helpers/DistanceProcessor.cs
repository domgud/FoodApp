using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FoodApp.Models;
namespace FoodApp.Helpers
{
    public class DistanceProcessor
    {
        public async Task<DistanceModel> LoadDistance(string origin, string destination)
        {
            string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origin}&destinations={destination}&key={APIHelper.googleapikey}";
            using (HttpResponseMessage response = await APIHelper.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    DistanceModel distance = await response.Content.ReadAsAsync<DistanceModel>();

                    return distance;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
