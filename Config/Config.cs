﻿namespace P2P.Config
{
    public class Config
    {
        private const string strUrlServer = @"https://localhost:7114/";
        //private const string strUrlServer = @"https://milkteaheaven-g9gzbdgzamhwgmap.eastasia-01.azurewebsites.net/";
        public static string Connection()
        {
            //string conn = @"Data Source=tcp:havansang.database.windows.net,1433;Initial Catalog=MilkTeaShop;User ID=sang;Password=Sh123456;Trust Server Certificate=True";

            string conn = @"Data Source=MSI\SQLEXPRESS;Initial Catalog=P2P;User ID=sang;Password=123456;Trust Server Certificate=True";
            return conn;
        }

        //public static string ImageUrl()
        //{
        //    string imageUrl = strUrlServer;
        //    return imageUrl;
        //}

        //public static string ProductImageUrl()
        //{
        //    string imageUrl = $"{strUrlServer}Images/Product/";
        //    return imageUrl;
        //}

        //public static string GoodsReceiptUrl()
        //{
        //    string imageUrl = $"{strUrlServer}GoodsReceipt/";
        //    return imageUrl;
        //}

        //public static string GoodsIssuetUrl()
        //{
        //    string imageUrl = $"{strUrlServer}GoodsIssues/";
        //    return imageUrl;
        //}
    }
}
