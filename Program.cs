// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using UPA_EXTERNAL_MODELS;
using UPA_EXTERNAL_MODELS.Models.BloodBanks;
using UPA_EXTERNAL_MODELS.Models.Configs;
using UPA_EXTERNAL_MODELS.Models.Customer;
using UPA_SDK;
using UPAExternalAPI.Models.Customer;
using UPAExternalAPI.Models.Inventory;
using UPAExternalAPI.Models.Vendor;

internal class Program
{
    private static void Main(string[] args)
    {
        /// You Need to add reference to UPA_EXTERNAL_MODELS project published at [https://github.com/upa-dt/UPA_EXTERNAL_MODELS]
        /// You Need to add reference to UPA_SDK project published at [https://github.com/upa-dt/UPA_SDK] 
        /// Add YOUR Provided Authentication Data to file "config.json" and this project will be functional
        /// Current Values In the config.json are Sample Authentication Data and will not Work
        LocalConfig conf = JsonConvert.DeserializeObject<LocalConfig>(File.ReadAllText("config.json"));
        RuntimeVariables _runtime = JsonConvert.DeserializeObject<RuntimeVariables>(File.ReadAllText("Runtime.json"));
        Customer _customerObject = new Customer(conf.API_ROOT_URL,
                                 conf.API_KEY,
                                 conf.API_USER,
                                 conf.API_PASSWORD,
                                 conf.API_OTP_SECRET,
                                 conf.API_SECRET,
                                 15);
        //Login to the API server
        var reso = _customerObject.Login();
        if (!reso)// Failure Login
        {
            Console.WriteLine("FAILED Login");
            return;
        }

        /// Uncomment Next Line to iterate through all Purchase Requests
        Console.WriteLine("************** SEARCH PURCHASE REQUESTS **************");
        //Search_Purchase_Requests(_runtime, _customerObject);

        int pr_number = 122; // this is a sample Purchase request number, It Must belongs to you or one of your children, You Can get one from result of PrSearchRequest Results

        // Uncomment Next Line to Download Purchase Request pdf
        Console.WriteLine($"************** DOWNLOAD PDF OF PURCHASE REQUEST {pr_number} **************");
        //_customerObject.PrDownload(pr_number, $"./{pr_number}.pdf");


        /// Get Meta Data Of  aPurchase Request (approvals and Main PR Data)
        Console.WriteLine($"************** Get Meta Data Of PURCHASE REQUEST {pr_number} **************");
        var prMeta = _customerObject.PrGetMetaData(pr_number);
        Console.WriteLine(prMeta.SerializeObject());

        /// Get Detailed Items Of Purchase Request
        var prItems = _customerObject.PrGetItems(pr_number);
        Console.WriteLine($"************** ALL ITEMS Of PURCHASE REQUEST {pr_number} **************");
        foreach (var item in prItems.Result)
        {
            /// Loop Through Purchase Request Items
            Console.WriteLine(item.SerializeObject());
        }

        ///Get All Purchase Orders Generated for this Purchase Request
        var pr_po_list = _customerObject.PrGetPoList(pr_number);
        /// Loop through all Purchase Orders of a specific Purchase Request
        Console.WriteLine($"************** ALL PURCHASE ORDERS Of PURCHASE REQUEST {pr_number} **************");
        foreach (var item in pr_po_list.Result)
        {
            /// Loop Through Purchase Request Items
            Console.WriteLine(item.SerializeObject());
        }

        // Take first Purchase Order of the fixed Purchase Request (pr_number)DONT FORGET SETTING IT TO ONE OF YOUR PYRCHASE REQUESTS
        int po = pr_po_list.Result.First().Po;

        /// Download First Purchase Order of 
        /// Uncomment Next Line to Download Purchase ORder For the Specified Ourchase Request
        Console.WriteLine($"************** DOWNLOAD PDF OF  PART OF PURCHASE ORDER {po} DEDICATED TO PURCHASE REQUEST {pr_number} **************");
        //_customerObject.Pr_Po_Download(pr_number, po, $"{pr_number}_{po}.pdf");

        var po_det = _customerObject.PrGetPoDet(pr_number, po);
        ///Loop through all Po items of pr request
        Console.WriteLine($"************** GET ALL DETAILED ITEMS OF PURCHASE ORDER {po} DEDICATED TO PURCHASE REQUEST {pr_number} **************");
        foreach (var item in po_det.Result)
        {
            /// Loop Through Purchase Request Items
            Console.WriteLine(item.SerializeObject());
        }

        /// Get Distribution List for a purchase request
        int purchase_detail_id = prItems.Result.First().PrDetId;
        var pr_destribution = _customerObject.PrGetPrDetDistribution(pr_number, purchase_detail_id);
        Console.WriteLine($"************** GET ALL DISTRIBUTION LIST OF PURCHASE REQUEST  {pr_number} AMONG ALL PURCHASE ORDERS **************");
        foreach (var item in pr_destribution.Result)
        {
            /// Loop Through Purchase Order  Items Distribution List
            Console.WriteLine(item.SerializeObject());
        }

        /// List of reception Invoices for a Purchase Request
        var pr_ri_list = _customerObject.PrGetRiList(pr_number);
        Console.WriteLine($"************** GET ALL DISTRIBUTION LIST OF RECEPTION INVOICES FOR PURCHASE REQUEST NUMBER {pr_number} **************");
        foreach (var item in pr_ri_list.Result)
        {
            /// Loop Through Purchase Order  Items Distribution List
            Console.WriteLine(item.SerializeObject());
        }


        int ri = pr_ri_list.Result.First().Ri;
        var ri_det_list = _customerObject.RiGetItems(ri);

        Console.WriteLine($"************** GET LIST OF ITEMS INSIDE A RECEPTION INVOICES {ri} **************");
        foreach (var item in ri_det_list.Result)
        {
            /// Loop Through Purchase Order  Items Distribution List
            Console.WriteLine(item.SerializeObject());
        }
        Console.WriteLine($"************** FIRST ACCEPTANCE OF RECEPTION INVOICES {ri} **************");

        /// Receive Reception Invoice
        List<string> invImages = new List<string> {
            "D:\\Work-Backups\\UPSMT\\REC_INV_IMGS\\Mobile_Uploads\\531141\\inv_0.jpeg" ,// Path To Images
        "D:\\Work-Backups\\UPSMT\\REC_INV_IMGS\\Mobile_Uploads\\531141\\inv_0.jpeg" ,
        "D:\\Work-Backups\\UPSMT\\REC_INV_IMGS\\Mobile_Uploads\\531141\\inv_0.jpeg" ,};
        List<string> riImages = new List<string> {
            "D:\\Work-Backups\\UPSMT\\REC_INV_IMGS\\Mobile_Uploads\\531141\\doc_0.jpeg" ,
        "D:\\Work-Backups\\UPSMT\\REC_INV_IMGS\\Mobile_Uploads\\531141\\doc_0.jpeg",
        "D:\\Work-Backups\\UPSMT\\REC_INV_IMGS\\Mobile_Uploads\\531141\\doc_0.jpeg"};
        List<int> refused = new List<int>();// Here You Can Add Item Id of Reception Invoice  ri_det_list.Result[refused item index].RiItemId
        _customerObject.RiReceive(ri, refused, invImages, riImages);
        Console.WriteLine("RiReceive DONE===>");
        Console.WriteLine($"************** FINAL ACCEPTANCE OF RECEPTION INVOICES {ri} **************");

        /// Accept Reception Invoice After Inspection
        List<int> inspection_refused = new List<int>();// Here You Can Add Item Id of Reception Invoice  ri_det_list.Result[refused item index].RiItemId
        _customerObject.RiAccept(ri, inspection_refused, invImages);
        Console.WriteLine("Acceptance DONE===>");


        int warehouse_id = prMeta.Result.Info[0].WarehouseId;
        Console.WriteLine($"************** INVENTORY {warehouse_id} SEARCH **************");
        InventorySearchRequest invSearchParams = new InventorySearchRequest
        {
            custdyType = null,// CustodyTypeEnum.VENDOR_CUSTDY,
            MaxCount = 0,
            Offset = 0,
            productType = null, // ProductTypeEnum.Drugs,
            warehouseId = warehouse_id
        };
        var inventory = _customerObject.InvSearch(invSearchParams);
        foreach (var item in inventory.Result)
        {
            /// Loop Through All Inventory Items
            Console.WriteLine(item.SerializeObject());
        }
        var _target_transaction = inventory.Result.Last().InventoryTransactionId;
        Console.WriteLine($"************** INVENTORY {warehouse_id} ADD TRANSACTION {_target_transaction} **************");

        InventoryTransactionRequest _new_Transaction = new InventoryTransactionRequest
        {
            destWarehouseId = warehouse_id,
            qty = 1,
            parentTransactionId = _target_transaction,
            bonus = 0,
            srcCustodyType = CustodyTypeEnum.WAREHOUSE_CUSTDY,
            transcationType = TransactionTypeEnum.ENTITY_CONSUMPTION
        };
        var tr_result = _customerObject.InventoryAddTransaction(_new_Transaction);
        Console.WriteLine(tr_result.Result);

        Console.WriteLine($"************** INVENTORY {warehouse_id} ADD Inspection **************");
        InventoryInspectionRequest _inspectionData = new InventoryInspectionRequest
        {
            cur_qty = 100, // Current Quantity Of The Transaction
            parentTransactionId = _target_transaction
        };
        var inv_add_trans_result = _customerObject.InventoryInspect(_inspectionData);
        Console.WriteLine(inv_add_trans_result.Result);

        //Console.WriteLine(pr_po_list.SerializeObject());
        Console.WriteLine("**************** THE END ****************************");
    }

    private static void Search_Purchase_Requests(RuntimeVariables _runtime, Customer _customerObject)
    {
        PrSearchRequest searchFilter = new PrSearchRequest
        {
            Offset = _runtime.LatestPoOffset,//Offset of records to return
            IncludeChildren = true,// Include children of this customer or not
            //FilterPr = 50, // You can provide this filter Purchase Request to get this specific one
            //FromDate = new DateTime(1970, 1, 1), // You can provide this filter Start Date to search
            //ToDate = DateTime.Now,// You can provide this filter End Date to search
            //MaxCount = 50,// Maximum Returned Count of records
        };
        ResponseContainer<List<PrData>> result = _customerObject.PrSearch(searchFilter);
        while (result != null && result.resultLength > 0)
        {
            searchFilter.Offset += result.resultLength;
            _runtime.LatestPoOffset = searchFilter.Offset.Value;
            foreach (var item in result.Result)
            {
                /// item Here Contains Pr Data
                /// Here You Can Loop through All Purchase Requests
                Console.WriteLine(item.SerializeObject());
            }
            File.WriteAllText("Runtime.json", _runtime.SerializeObject());
            result = _customerObject.PrSearch(searchFilter);
        }
    }


}