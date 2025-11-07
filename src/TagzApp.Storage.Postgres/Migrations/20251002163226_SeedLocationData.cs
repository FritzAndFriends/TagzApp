using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TagzApp.Storage.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SeedLocationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Name", "Latitude", "Longitude" },
                values: new object[,]
                {
                    { "A Coruña", 43.3623m, -8.4115m },
                    { "Abbotsford", 49.0580m, -122.2915m },
                    { "Aberdeen", 57.1497m, -2.0943m },
                    { "Acapulco", 16.8531m, -99.8237m },
                    { "Adoni", 15.6281m, 77.2750m },
                    { "Agartala", 23.8315m, 91.2868m },
                    { "Agra", 27.1767m, 78.0081m },
                    { "Aguascalientes", 21.8853m, -102.2916m },
                    { "Ahmedabad", 23.0225m, 72.5714m },
                    { "Ahmednagar", 19.0948m, 74.7480m },
                    { "Aizawl", 23.7271m, 92.7176m },
                    { "Ajax", 43.8509m, -79.0204m },
                    { "Ajmer", 26.4499m, 74.6399m },
                    { "Akola", 20.7002m, 77.0082m },
                    { "Akron", 41.0814m, -81.5190m },
                    { "Albuquerque", 35.0844m, -106.6504m },
                    { "Alexandria", 31.2001m, 29.9187m },
                    { "Aligarh", 27.8974m, 78.0880m },
                    { "Allahabad", 25.4358m, 81.8463m },
                    { "Alwar", 27.5530m, 76.6346m },
                    { "Amarillo", 35.2220m, -101.8313m },
                    { "Ambattur", 13.1143m, 80.1548m },
                    { "Ambernath", 19.1861m, 73.1997m },
                    { "Amravati", 20.9374m, 77.7796m },
                    { "Amritsar", 31.6340m, 74.8723m },
                    { "Amroha", 28.9034m, 78.4670m },
                    { "Amsterdam", 52.3676m, 4.9041m },
                    { "Anaheim", 33.8366m, -117.9143m },
                    { "Anantapur", 14.6819m, 77.6006m },
                    { "Anchorage", 61.2181m, -149.9003m },
                    { "Ankara", 39.9334m, 32.8597m },
                    { "Antwerp", 51.2194m, 4.4025m },
                    { "Arlington", 32.7357m, -97.1081m },
                    { "Arrah", 25.5581m, 84.6628m },
                    { "Asansol", 23.6739m, 86.9524m },
                    { "Athens", 37.9838m, 23.7275m },
                    { "Atlanta", 33.7490m, -84.3880m },
                    { "Augusta", 33.4735m, -82.0105m },
                    { "Aurangabad", 19.8762m, 75.3433m },
                    { "Aurora", 39.7294m, -104.8319m },
                    { "Aurora IL", 41.7606m, -88.3201m },
                    { "Austin", 30.2672m, -97.7431m },
                    { "Avadi", 13.1147m, 80.0982m },
                    { "Badalona", 41.4502m, 2.2445m },
                    { "Baghdad", 33.3152m, 44.3661m },
                    { "Bahadurgarh", 28.6928m, 76.9378m },
                    { "Bakersfield", 35.3733m, -119.0187m },
                    { "Bally", 22.6503m, 88.3409m },
                    { "Baltimore", 39.2904m, -76.6122m },
                    { "Bangalore", 12.9716m, 77.5946m },
                    { "Bangkok", 13.7563m, 100.5018m },
                    { "Barabanki", 26.9238m, 81.2041m },
                    { "Baranagar", 22.6417m, 88.3736m },
                    { "Bardhaman", 23.2324m, 87.8615m },
                    { "Bareilly", 28.3670m, 79.4304m },
                    { "Barrie", 44.3894m, -79.6903m },
                    { "Bathinda", 30.2110m, 74.9455m },
                    { "Baton Rouge", 30.4515m, -91.1871m },
                    { "Begusarai", 25.4182m, 86.1272m },
                    { "Beijing", 39.9042m, 116.4074m },
                    { "Belfast", 54.5973m, -5.9301m },
                    { "Belgaum", 15.8497m, 74.4977m },
                    { "Bellary", 15.1394m, 76.9214m },
                    { "Belo Horizonte", -19.9167m, -43.9345m },
                    { "Berhampur", 19.3149m, 84.7941m },
                    { "Berlin", 52.5200m, 13.4050m },
                    { "Bhagalpur", 25.2425m, 86.9842m },
                    { "Bhagha Purana", 30.6715m, 75.4726m },
                    { "Bharatpur", 27.2152m, 77.4909m },
                    { "Bhatpara", 22.8736m, 88.4009m },
                    { "Bhavnagar", 21.7645m, 72.1519m },
                    { "Bhilai Nagar", 21.1938m, 81.3509m },
                    { "Bhilwara", 25.3407m, 74.6269m },
                    { "Bhiwandi", 19.3002m, 73.0621m },
                    { "Bhopal", 23.2599m, 77.4126m },
                    { "Bhubaneswar", 20.2961m, 85.8245m },
                    { "Bihar Sharif", 25.2013m, 85.5226m },
                    { "Bijapur", 16.8302m, 75.7100m },
                    { "Bikaner", 28.0229m, 73.3119m },
                    { "Bilaspur", 22.0797m, 82.1409m },
                    { "Bilbao", 43.2627m, -2.9253m },
                    { "Birmingham", 52.4862m, -1.8904m },
                    { "Birmingham AL", 33.5207m, -86.8025m },
                    { "Bogotá", 4.7110m, -74.0721m },
                    { "Boise", 43.6150m, -116.2023m },
                    { "Bokaro", 23.6693m, 86.1511m },
                    { "Bordeaux", 44.8378m, -0.5792m },
                    { "Boston", 42.3601m, -71.0589m },
                    { "Bradford", 53.7960m, -1.7594m },
                    { "Brampton", 43.7315m, -79.7624m },
                    { "Brasília", -15.8267m, -47.9218m },
                    { "Bremen", 53.0793m, 8.8017m },
                    { "Bristol", 51.4545m, -2.5879m },
                    { "Brussels", 50.8503m, 4.3517m },
                    { "Bucharest", 44.4268m, 26.1025m },
                    { "Budapest", 47.4979m, 19.0402m },
                    { "Buenos Aires", -34.6037m, -58.3816m },
                    { "Burlington", 43.3255m, -79.7990m },
                    { "Burnaby", 49.2488m, -122.9805m },
                    { "Buxar", 25.5648m, 83.9784m },
                    { "Cairo", 30.0444m, 31.2357m },
                    { "Calgary", 51.0447m, -114.0719m },
                    { "Cambridge", 43.3616m, -80.3144m },
                    { "Cancún", 21.1619m, -86.8515m },
                    { "Cardiff", 51.4816m, -3.1791m },
                    { "Cartagena", 37.6063m, -0.9836m },
                    { "Chandigarh", 30.7333m, 76.7794m },
                    { "Chandler", 33.3062m, -111.8413m },
                    { "Chandrapur", 19.9615m, 79.2961m },
                    { "Changchun", 43.8171m, 125.3235m },
                    { "Changsha", 28.2282m, 112.9388m },
                    { "Charlotte", 35.2271m, -80.8431m },
                    { "Chatham-Kent", 42.4045m, -82.1915m },
                    { "Chengdu", 30.5728m, 104.0668m },
                    { "Chennai", 13.0827m, 80.2707m },
                    { "Chesapeake", 36.7682m, -76.2875m },
                    { "Chicago", 41.8781m, -87.6298m },
                    { "Chihuahua", 28.6353m, -106.0889m },
                    { "Chimalhuacán", 19.4202m, -98.9540m },
                    { "Chongqing", 29.5630m, 106.5516m },
                    { "Chula Vista", 32.6401m, -117.0842m },
                    { "Cincinnati", 39.1031m, -84.5120m },
                    { "Cleveland", 41.4993m, -81.6944m },
                    { "Coimbatore", 11.0168m, 76.9558m },
                    { "Cologne", 50.9375m, 6.9603m },
                    { "Colorado Springs", 38.8339m, -104.8214m },
                    { "Columbus", 39.9612m, -82.9988m },
                    { "Columbus GA", 32.4609m, -84.9877m },
                    { "Copenhagen", 55.6761m, 12.5683m },
                    { "Coquitlam", 49.3273m, -122.7816m },
                    { "Córdoba", 37.8882m, -4.7794m },
                    { "Corpus Christi", 27.8006m, -97.3964m },
                    { "Coventry", 52.4068m, -1.5197m },
                    { "Cuernavaca", 18.9219m, -99.2419m },
                    { "Culiacán", 24.7999m, -107.3943m },
                    { "Cuttack", 20.4625m, 85.8830m },
                    { "Dalian", 38.9140m, 121.6147m },
                    { "Dallas", 32.7767m, -96.7970m },
                    { "Dar es Salaam", -6.7924m, 39.2083m },
                    { "Darbhanga", 26.1542m, 85.8918m },
                    { "Davanagere", 14.4644m, 75.9077m },
                    { "Dehradun", 30.3165m, 78.0322m },
                    { "Delhi", 28.7041m, 77.1025m },
                    { "Delta", 49.0847m, -123.0585m },
                    { "Denver", 39.7392m, -104.9903m },
                    { "Derby", 52.9225m, -1.4746m },
                    { "Des Moines", 41.5868m, -93.6250m },
                    { "Detroit", 42.3314m, -83.0458m },
                    { "Dewas", 22.9676m, 76.0534m },
                    { "Dhaka", 23.8103m, 90.4125m },
                    { "Dhanbad", 23.7957m, 86.4304m },
                    { "Dhule", 20.9042m, 74.7749m },
                    { "Dibrugarh", 27.4728m, 94.9120m },
                    { "Dieppe", 46.0769m, -64.6837m },
                    { "Dongguan", 23.0489m, 113.7447m },
                    { "Dortmund", 51.5136m, 7.4653m },
                    { "Dresden", 51.0504m, 13.7373m },
                    { "Dundee", 56.4620m, -2.9707m },
                    { "Durango", 24.0277m, -104.6532m },
                    { "Durg", 21.1900m, 81.2849m },
                    { "Durgapur", 23.4820m, 87.3119m },
                    { "Durham", 35.9940m, -78.8986m },
                    { "Düsseldorf", 51.2277m, 6.7735m },
                    { "Edinburgh", 55.9533m, -3.1883m },
                    { "Edmonton", 53.5461m, -113.4938m },
                    { "El Paso", 31.7619m, -106.4850m },
                    { "Elche", 38.2622m, -0.7079m },
                    { "Eluru", 16.7107m, 81.1040m },
                    { "English Bazar", 25.0119m, 88.1495m },
                    { "Ensenada", 31.8665m, -116.5956m },
                    { "Erode", 11.3410m, 77.7172m },
                    { "Essen", 51.4556m, 7.0116m },
                    { "Etawah", 26.7753m, 79.0154m },
                    { "Faridabad", 28.4089m, 77.3178m },
                    { "Farrukhabad", 27.3974m, 79.5809m },
                    { "Fayetteville", 35.0527m, -78.8784m },
                    { "Firozabad", 27.1592m, 78.3957m },
                    { "Florence", 43.7696m, 11.2558m },
                    { "Fontana", 34.0922m, -117.4350m },
                    { "Fort Wayne", 41.0793m, -85.1394m },
                    { "Fort Worth", 32.7555m, -97.3308m },
                    { "Foshan", 23.0218m, 113.1219m },
                    { "Frankfurt", 50.1109m, 8.6821m },
                    { "Fremont", 37.5485m, -121.9886m },
                    { "Fresno", 36.7378m, -119.7871m },
                    { "Gandhidham", 23.0800m, 70.1300m },
                    { "Garland", 32.9126m, -96.6389m },
                    { "Gatineau", 45.4765m, -75.7013m },
                    { "Gaya", 24.7914m, 85.0002m },
                    { "Gdańsk", 54.3520m, 18.6466m },
                    { "Genoa", 44.4056m, 8.9463m },
                    { "Ghaziabad", 28.6692m, 77.4538m },
                    { "Gijón", 43.5322m, -5.6611m },
                    { "Gilbert", 33.3528m, -111.7890m },
                    { "Giza", 30.0131m, 31.2089m },
                    { "Glasgow", 55.8642m, -4.2518m },
                    { "Glendale", 33.5387m, -112.1860m },
                    { "Gopalpur", 22.4741m, 88.3328m },
                    { "Gothenburg", 57.7089m, 11.9746m },
                    { "Granada", 37.1773m, -3.5986m },
                    { "Grand Rapids", 42.9634m, -85.6681m },
                    { "Greensboro", 36.0726m, -79.7920m },
                    { "Guadalajara", 20.6597m, -103.3496m },
                    { "Guangzhou", 23.1291m, 113.2644m },
                    { "Guelph", 43.5448m, -80.2482m },
                    { "Gulbarga", 17.3297m, 76.8343m },
                    { "Guntur", 16.3067m, 80.4365m },
                    { "Gurgaon", 28.4595m, 77.0266m },
                    { "Guwahati", 26.1445m, 91.7362m },
                    { "Gwalior", 26.2183m, 78.1828m },
                    { "Halifax", 44.6488m, -63.5752m },
                    { "Hamburg", 53.5511m, 9.9937m },
                    { "Hamilton", 43.2557m, -79.8711m },
                    { "Hangzhou", 30.2741m, 120.1551m },
                    { "Hannover", 52.3759m, 9.7320m },
                    { "Hanoi", 21.0285m, 105.8542m },
                    { "Hapur", 28.7298m, 77.7761m },
                    { "Harbin", 45.8038m, 126.5349m },
                    { "Hefei", 31.8639m, 117.2808m },
                    { "Helsinki", 60.1699m, 24.9384m },
                    { "Henderson", 36.0395m, -114.9817m },
                    { "Hermosillo", 29.0729m, -110.9559m },
                    { "Hialeah", 25.8576m, -80.2781m },
                    { "Ho Chi Minh City", 10.8231m, 106.6297m },
                    { "Hong Kong", 22.3193m, 114.1694m },
                    { "Honolulu", 21.3099m, -157.8581m },
                    { "Hospet", 15.2687m, 76.3892m },
                    { "Houston", 29.7604m, -95.3698m },
                    { "Howrah", 22.5958m, 88.2636m },
                    { "Hubballi-Dharwad", 15.3647m, 75.1240m },
                    { "Huntington Beach", 33.6603m, -117.9992m },
                    { "Huntsville", 34.7304m, -86.5861m },
                    { "Hyderabad", 17.3850m, 78.4867m },
                    { "Ichalkaranji", 16.6891m, 74.4606m },
                    { "Imphal", 24.8170m, 93.9368m },
                    { "Indianapolis", 39.7684m, -86.1581m },
                    { "Indore", 22.7196m, 75.8577m },
                    { "Irapuato", 20.6767m, -101.3542m },
                    { "Irvine", 33.6846m, -117.8265m },
                    { "Irving", 32.8140m, -96.9489m },
                    { "Istanbul", 41.0082m, 28.9784m },
                    { "Jabalpur", 23.1815m, 79.9864m },
                    { "Jacksonville", 30.3322m, -81.6557m },
                    { "Jaipur", 26.9124m, 75.7873m },
                    { "Jakarta", -6.2088m, 106.8456m },
                    { "Jalandhar", 31.3260m, 75.5762m },
                    { "Jalgaon", 21.0077m, 75.5626m },
                    { "Jalna", 19.8347m, 75.8861m },
                    { "Jammu", 32.7266m, 74.8570m },
                    { "Jamnagar", 22.4707m, 70.0577m },
                    { "Jamshedpur", 22.8046m, 86.2029m },
                    { "Jehanabad", 25.2118m, 84.9883m },
                    { "Jerez de la Frontera", 36.6868m, -6.1362m },
                    { "Jersey City", 40.7178m, -74.0431m },
                    { "Jhansi", 25.4484m, 78.5685m },
                    { "Jinan", 36.6512m, 117.1201m },
                    { "Jodhpur", 26.2389m, 73.0243m },
                    { "Johannesburg", -26.2041m, 28.0473m },
                    { "Juárez", 31.6904m, -106.4245m },
                    { "Junagadh", 21.5222m, 70.4579m },
                    { "Kadapa", 14.4673m, 78.8242m },
                    { "Kakinada", 16.9891m, 82.2475m },
                    { "Kalyan-Dombivali", 19.2403m, 73.1305m },
                    { "Kamarhati", 22.6708m, 88.3742m },
                    { "Kanpur", 26.4499m, 80.3319m },
                    { "Kansas City", 39.0997m, -94.5786m },
                    { "Karachi", 24.8607m, 67.0011m },
                    { "Karimnagar", 18.4386m, 79.1288m },
                    { "Karnal", 29.6857m, 76.9905m },
                    { "Katihar", 25.5394m, 87.5831m },
                    { "Kelowna", 49.8880m, -119.4960m },
                    { "Khammam", 17.2473m, 80.1514m },
                    { "Khartoum", 15.5007m, 32.5599m },
                    { "Kinshasa", -4.4419m, 15.2663m },
                    { "Kirari Suleman Nagar", 28.7233m, 77.0400m },
                    { "Kitchener", 43.4516m, -80.4925m },
                    { "Kochi", 9.9312m, 76.2673m },
                    { "Kolhapur", 16.7050m, 74.2433m },
                    { "Kolkata", 22.5726m, 88.3639m },
                    { "Kollam", 8.8932m, 76.6141m },
                    { "Korba", 22.3595m, 82.7501m },
                    { "Kota", 25.2138m, 75.8648m },
                    { "Kozhencherry", 9.2981m, 76.7761m },
                    { "Kozhikode", 11.2588m, 75.7804m },
                    { "Kraków", 50.0647m, 19.9450m },
                    { "Krishnanagar", 23.4058m, 88.4818m },
                    { "Kuala Lumpur", 3.1390m, 101.6869m },
                    { "Kulti", 23.7307m, 86.8451m },
                    { "Kunming", 25.0389m, 102.7183m },
                    { "Kurnool", 15.8281m, 78.0373m },
                    { "L'Hospitalet", 41.3598m, 2.1074m },
                    { "Lagos", 6.5244m, 3.3792m },
                    { "Lahore", 31.5497m, 74.3436m },
                    { "Langley", 49.1042m, -122.6604m },
                    { "Lanzhou", 36.0611m, 103.8343m },
                    { "Laredo", 27.5306m, -99.4803m },
                    { "Las Palmas", 28.1248m, -15.4300m },
                    { "Las Vegas", 36.1699m, -115.1398m },
                    { "Las Venturas", 36.1699m, -115.1398m },
                    { "Latur", 18.4088m, 76.5604m },
                    { "Laval", 45.6066m, -73.7124m },
                    { "Leeds", 53.8008m, -1.5491m },
                    { "Leicester", 52.6369m, -1.1398m },
                    { "Leipzig", 51.3397m, 12.3731m },
                    { "León", 21.1619m, -101.6921m },
                    { "Lévis", 46.8000m, -71.1772m },
                    { "Lexington", 38.0406m, -84.5037m },
                    { "Lille", 50.6292m, 3.0573m },
                    { "Lima", -12.0464m, -77.0428m },
                    { "Lincoln", 40.8136m, -96.7026m },
                    { "Little Rock", 34.7465m, -92.2896m },
                    { "Liverpool", 53.4084m, -2.9916m },
                    { "Łódź", 51.7592m, 19.4560m },
                    { "London", 51.5074m, -0.1278m },
                    { "London ON", 42.9849m, -81.2453m },
                    { "Long Beach", 33.7701m, -118.1937m },
                    { "Longueuil", 45.5312m, -73.5185m },
                    { "Loni", 28.7333m, 77.2833m },
                    { "Los Angeles", 34.0522m, -118.2437m },
                    { "Los Santos", 34.0522m, -118.2437m },
                    { "Louisville", 38.2527m, -85.7585m },
                    { "Luanda", -8.8390m, 13.2894m },
                    { "Lubbock", 33.5779m, -101.8552m },
                    { "Lucknow", 26.8467m, 80.9462m },
                    { "Ludhiana", 30.9010m, 75.8573m },
                    { "Lyon", 45.7640m, 4.8357m },
                    { "Madison", 43.0731m, -89.4012m },
                    { "Madrid", 40.4168m, -3.7038m },
                    { "Madurai", 9.9252m, 78.1198m },
                    { "Mahbubnagar", 16.7430m, 77.9982m },
                    { "Maheshtala", 22.4991m, 88.2481m },
                    { "Málaga", 36.7213m, -4.4214m },
                    { "Malegaon", 20.5579m, 74.5287m },
                    { "Mamit", 23.9306m, 92.4781m },
                    { "Manchester", 53.4808m, -2.2426m },
                    { "Mandsaur", 24.0770m, 75.0700m },
                    { "Mandurah", -32.5269m, 115.7721m },
                    { "Mangalore", 12.9141m, 74.8560m },
                    { "Manila", 14.5995m, 120.9842m },
                    { "Markham", 43.8561m, -79.3370m },
                    { "Marseille", 43.2965m, 5.3698m },
                    { "Matamoros", 25.8698m, -97.504m },
                    { "Mathura", 27.4924m, 77.6737m },
                    { "Mau", 25.9420m, 83.5611m },
                    { "Mazatlán", 23.2494m, -106.4103m },
                    { "Medellín", 6.2442m, -75.5812m },
                    { "Meerut", 28.9845m, 77.7064m },
                    { "Melbourne", -37.8136m, 144.9631m },
                    { "Memphis", 35.1495m, -90.0490m },
                    { "Mérida", 20.9674m, -89.5926m },
                    { "Mesa", 33.4152m, -111.8315m },
                    { "Mexicali", 32.6245m, -115.4523m },
                    { "Mexico City", 19.4326m, -99.1332m },
                    { "Miami", 25.7617m, -80.1918m },
                    { "Milan", 45.4642m, 9.1900m },
                    { "Milton", 43.5183m, -79.8774m },
                    { "Milwaukee", 43.0389m, -87.9065m },
                    { "Minneapolis", 44.9778m, -93.2650m },
                    { "Mira-Bhayandar", 19.2953m, 72.8540m },
                    { "Mirzapur", 25.1463m, 82.5693m },
                    { "Mississauga", 43.5890m, -79.6441m },
                    { "Mobile", 30.6954m, -88.0399m },
                    { "Modesto", 37.6391m, -120.9969m },
                    { "Moncton", 46.0878m, -64.7782m },
                    { "Monterrey", 25.6866m, -100.3161m },
                    { "Montgomery", 32.3617m, -86.2792m },
                    { "Montpellier", 43.6110m, 3.8767m },
                    { "Montreal", 45.5017m, -73.5673m },
                    { "Moradabad", 28.8386m, 78.7733m },
                    { "Morelia", 19.7069m, -101.1956m },
                    { "Moreno Valley", 33.9425m, -117.2297m },
                    { "Moscow", 55.7558m, 37.6176m },
                    { "Muktsar", 30.4762m, 74.5154m },
                    { "Mumbai", 19.0760m, 72.8777m },
                    { "Munich", 48.1351m, 11.5820m },
                    { "Murcia", 37.9922m, -1.1307m },
                    { "Muzaffarnagar", 29.4727m, 77.7085m },
                    { "Muzaffarpur", 26.1197m, 85.3910m },
                    { "Mysore", 12.2958m, 76.6394m },
                    { "Nagoya", 35.1815m, 136.9066m },
                    { "Nagpur", 21.1458m, 79.0882m },
                    { "Naihati", 22.8964m, 88.4197m },
                    { "Nanded", 19.1383m, 77.3210m },
                    { "Nanjing", 32.0603m, 118.7969m },
                    { "Nantes", 47.2184m, -1.5536m },
                    { "Naples", 40.8518m, 14.2681m },
                    { "Nashik", 19.9975m, 73.7898m },
                    { "Nashville", 36.1627m, -86.7816m },
                    { "Naucalpan", 19.4775m, -99.2386m },
                    { "Navi Mumbai", 19.0330m, 73.0297m },
                    { "New Delhi", 28.6139m, 77.2090m },
                    { "New Orleans", 29.9511m, -90.0715m },
                    { "New York", 40.7128m, -74.0060m },
                    { "Newark", 40.7357m, -74.1724m },
                    { "Newcastle", 54.9783m, -1.6178m },
                    { "Nice", 43.7102m, 7.2620m },
                    { "Nizamabad", 18.6725m, 78.0941m },
                    { "Noida", 28.5355m, 77.3910m },
                    { "Norfolk", 36.8468m, -76.2852m },
                    { "North Dumdum", 22.6464m, 88.4096m },
                    { "North Las Vegas", 36.1989m, -115.1175m },
                    { "Norwich", 52.6309m, 1.2974m },
                    { "Nottingham", 52.9548m, -1.1581m },
                    { "Nuevo Laredo", 27.4758m, -99.5065m },
                    { "Nuremberg", 49.4521m, 11.0767m },
                    { "Oakland", 37.8044m, -122.2711m },
                    { "Oakville", 43.4675m, -79.6877m },
                    { "Oaxaca", 17.0732m, -96.7266m },
                    { "Oklahoma City", 35.4676m, -97.5164m },
                    { "Omaha", 41.2565m, -95.9345m },
                    { "Orlando", 28.5383m, -81.3792m },
                    { "Osaka", 34.6937m, 135.5023m },
                    { "Oshawa", 43.8971m, -78.8658m },
                    { "Oslo", 59.9139m, 10.7522m },
                    { "Ottawa", 45.4215m, -75.6972m },
                    { "Oviedo", 43.3614m, -5.8593m },
                    { "Oxnard", 34.1975m, -119.1771m },
                    { "Ozhukarai", 11.9416m, 79.7734m },
                    { "Pachuca", 20.1011m, -98.7591m },
                    { "Palermo", 38.1157m, 13.3613m },
                    { "Pali", 25.7711m, 73.3234m },
                    { "Palma", 39.5696m, 2.6502m },
                    { "Panihati", 22.6943m, 88.3712m },
                    { "Panipat", 29.3909m, 76.9635m },
                    { "Parbhani", 19.2608m, 76.7754m },
                    { "Paris", 48.8566m, 2.3522m },
                    { "Patiala", 30.3398m, 76.3869m },
                    { "Patna", 25.5941m, 85.1376m },
                    { "Peterborough", 52.5695m, -0.2405m },
                    { "Philadelphia", 39.9526m, -75.1652m },
                    { "Phoenix", 33.4484m, -112.0740m },
                    { "Pimpri-Chinchwad", 18.6298m, 73.7997m },
                    { "Pittsburgh", 40.4406m, -79.9959m },
                    { "Plano", 33.0198m, -96.6989m },
                    { "Plymouth", 50.3755m, -4.1427m },
                    { "Pondicherry", 11.9416m, 79.8083m },
                    { "Portland", 45.5152m, -122.6784m },
                    { "Portsmouth", 50.8198m, -1.0880m },
                    { "Poznań", 52.4064m, 16.9252m },
                    { "Prague", 50.0755m, 14.4378m },
                    { "Proddatur", 14.7504m, 78.5482m },
                    { "Puebla", 19.0414m, -98.2063m },
                    { "Pune", 18.5204m, 73.8567m },
                    { "Purnia", 25.7771m, 87.4753m },
                    { "Puruliya", 23.3424m, 86.3616m },
                    { "Qingdao", 36.0986m, 120.3719m },
                    { "Quebec City", 46.8139m, -71.2080m },
                    { "Querétaro", 20.5888m, -100.3899m },
                    { "Raichur", 16.2120m, 77.3439m },
                    { "Raipur", 21.2514m, 81.6296m },
                    { "Rajahmundry", 17.0005m, 81.8040m },
                    { "Rajkot", 22.3039m, 70.8022m },
                    { "Rajpur Sonarpur", 22.4615m, 88.4058m },
                    { "Raleigh", 35.7796m, -78.6382m },
                    { "Ramagundam", 18.4455m, 79.2948m },
                    { "Rampur", 28.8152m, 79.0177m },
                    { "Ranchi", 23.3441m, 85.3096m },
                    { "Ratlam", 23.3315m, 75.0367m },
                    { "Regina", 50.4452m, -104.6189m },
                    { "Reims", 49.2583m, 4.0317m },
                    { "Rennes", 48.1173m, -1.6778m },
                    { "Rewa", 24.5364m, 81.2961m },
                    { "Reynosa", 26.0756m, -98.2789m },
                    { "Richmond", 49.1666m, -123.1336m },
                    { "Richmond Hill", 43.8828m, -79.4403m },
                    { "Rio de Janeiro", -22.9068m, -43.1729m },
                    { "Riverside", 33.9533m, -117.3962m },
                    { "Riyadh", 24.7136m, 46.6753m },
                    { "Rochester", 43.1566m, -77.6088m },
                    { "Rohtak", 28.8955m, 76.6066m },
                    { "Rome", 41.9028m, 12.4964m },
                    { "Rotterdam", 51.9244m, 4.4777m },
                    { "Rourkela", 22.2604m, 84.8536m },
                    { "Saanich", 48.4833m, -123.3667m },
                    { "Sacramento", 38.5816m, -121.4944m },
                    { "Sagar", 23.8388m, 78.7378m },
                    { "Saguenay", 48.3844m, -71.0559m },
                    { "Saharanpur", 29.9680m, 77.5552m },
                    { "Saint Paul", 44.9537m, -93.0900m },
                    { "Saint Petersburg", 59.9311m, 30.3609m },
                    { "Salem", 11.6643m, 78.1460m },
                    { "Salford", 53.4875m, -2.2901m },
                    { "Salt Lake City", 40.7608m, -111.8910m },
                    { "Saltillo", 25.4260m, -101.0053m },
                    { "Sambalpur", 21.4669m, 83.9812m },
                    { "San Antonio", 29.4241m, -98.4936m },
                    { "San Bernardino", 34.1083m, -117.2898m },
                    { "San Diego", 32.7157m, -117.1611m },
                    { "San Francisco", 37.7749m, -122.4194m },
                    { "San Jose", 37.3382m, -121.8863m },
                    { "San Luis Potosí", 22.1565m, -100.9855m },
                    { "Sangli-Miraj & Kupwad", 16.8524m, 74.5815m },
                    { "Santa Ana", 33.7455m, -117.8677m },
                    { "Santiago", -33.4489m, -70.6693m },
                    { "São Paulo", -23.5505m, -46.6333m },
                    { "Saskatoon", 52.1579m, -106.6702m },
                    { "Satara", 17.6805m, 74.0183m },
                    { "Scottsdale", 33.4942m, -111.9261m },
                    { "Seattle", 47.6062m, -122.3321m },
                    { "Seoul", 37.5665m, 126.9780m },
                    { "Seville", 37.3891m, -5.9845m },
                    { "Shahjahanpur", 27.8831m, 79.9090m },
                    { "Shanghai", 31.2304m, 121.4737m },
                    { "Sheffield", 53.3811m, -1.4701m },
                    { "Shenyang", 41.8057m, 123.4315m },
                    { "Shenzhen", 22.5431m, 114.0579m },
                    { "Sherbrooke", 45.4042m, -71.8929m },
                    { "Shijiazhuang", 38.0428m, 114.5149m },
                    { "Shivamogga", 13.9299m, 75.5681m },
                    { "Shreveport", 32.5252m, -93.7502m },
                    { "Sikar", 27.6094m, 75.1399m },
                    { "Silchar", 24.8333m, 92.7789m },
                    { "Siliguri", 26.7271m, 88.3953m },
                    { "Singapore", 1.3521m, 103.8198m },
                    { "Sofia", 42.6977m, 23.3219m },
                    { "Solapur", 17.6599m, 75.9064m },
                    { "Sonipat", 28.9931m, 77.0151m },
                    { "South Dumdum", 22.6138m, 88.3931m },
                    { "Southampton", 50.9097m, -1.4044m },
                    { "Spokane", 47.6587m, -117.4260m },
                    { "Srinagar", 34.0837m, 74.7973m },
                    { "St. Catharines", 43.1594m, -79.2469m },
                    { "St. John's", 47.5615m, -52.7126m },
                    { "St. Louis", 38.6270m, -90.1994m },
                    { "St. Petersburg", 27.7676m, -82.6403m },
                    { "Stockholm", 59.3293m, 18.0686m },
                    { "Stockton", 37.9577m, -121.2908m },
                    { "Stoke-on-Trent", 53.0027m, -2.1794m },
                    { "Strasbourg", 48.5734m, 7.7521m },
                    { "Stuttgart", 48.7758m, 9.1829m },
                    { "Sunderland", 54.9069m, -1.3838m },
                    { "Surat", 21.1702m, 72.8311m },
                    { "Surrey", 49.1913m, -122.8490m },
                    { "Suzhou", 31.2989m, 120.5853m },
                    { "Swansea", 51.6214m, -3.9436m },
                    { "Sydney", -33.8688m, 151.2093m },
                    { "Tacoma", 47.2529m, -122.4443m },
                    { "Taiyuan", 37.8706m, 112.5489m },
                    { "Tallahassee", 30.4518m, -84.2807m },
                    { "Tampa", 27.9506m, -82.4572m },
                    { "Tampico", 22.2317m, -97.8674m },
                    { "Tehran", 35.6892m, 51.3890m },
                    { "Tenali", 16.2428m, 80.6455m },
                    { "Terrassa", 41.5648m, 2.0101m },
                    { "Terrebonne", 45.7057m, -73.6471m },
                    { "Thane", 19.2183m, 72.9781m },
                    { "Thanesar", 29.9735m, 76.8315m },
                    { "The Hague", 52.0705m, 4.3007m },
                    { "Thiruvananthapuram", 8.5241m, 76.9366m },
                    { "Thoothukudi", 8.7642m, 78.1348m },
                    { "Thrissur", 10.5276m, 76.2144m },
                    { "Thunder Bay", 48.3809m, -89.2477m },
                    { "Tianjin", 39.1042m, 117.1917m },
                    { "Tijuana", 32.5149m, -117.0382m },
                    { "Tiruchirappalli", 10.7905m, 78.7047m },
                    { "Tirunelveli", 8.7139m, 77.7567m },
                    { "Tirupur", 11.1085m, 77.3411m },
                    { "Tiruvottiyur", 13.1594m, 80.3008m },
                    { "Tlalnepantla", 19.5398m, -99.1944m },
                    { "Tlaquepaque", 20.6401m, -103.2893m },
                    { "Tokyo", 35.6762m, 139.6503m },
                    { "Toledo", 41.6528m, -83.5379m },
                    { "Toluca", 19.2926m, -99.6568m },
                    { "Toronto", 43.6532m, -79.3832m },
                    { "Torreón", 25.5428m, -103.4068m },
                    { "Toulouse", 43.6047m, 1.4442m },
                    { "Trois-Rivières", 46.3432m, -72.5477m },
                    { "Tucson", 32.2226m, -110.9747m },
                    { "Tulsa", 36.1540m, -95.9928m },
                    { "Tumkur", 13.3379m, 77.1025m },
                    { "Turin", 45.0703m, 7.6869m },
                    { "Tuxtla Gutiérrez", 16.7516m, -93.1161m },
                    { "Udaipur", 24.5854m, 73.7125m },
                    { "Ujjain", 23.1765m, 75.7885m },
                    { "Ulhasnagar", 19.2215m, 73.1645m },
                    { "Vadodara", 22.3072m, 73.1812m },
                    { "Valencia", 39.4699m, -0.3763m },
                    { "Valladolid", 41.6518m, -4.7245m },
                    { "Vancouver", 49.2827m, -123.1207m },
                    { "Varanasi", 25.3176m, 82.9739m },
                    { "Vasai-Virar", 19.4412m, 72.7973m },
                    { "Vaughan", 43.8361m, -79.4985m },
                    { "Veracruz", 19.1738m, -96.1342m },
                    { "Vice City", 25.7617m, -80.1918m },
                    { "Vienna", 48.2082m, 16.3738m },
                    { "Vigo", 42.2406m, -8.7207m },
                    { "Vijayawada", 16.5062m, 80.6480m },
                    { "Villahermosa", 17.9892m, -92.9475m },
                    { "Virginia Beach", 36.8529m, -75.9780m },
                    { "Visakhapatnam", 17.6868m, 83.2185m },
                    { "Vitoria-Gasteiz", 42.8467m, -2.6716m },
                    { "Warangal", 17.9689m, 79.5941m },
                    { "Warsaw", 52.2297m, 21.0122m },
                    { "Washington", 38.9072m, -77.0369m },
                    { "Waterloo", 43.4643m, -80.5204m },
                    { "Westminster", 51.4994m, -0.1269m },
                    { "Whitby", 43.8975m, -78.9429m },
                    { "Wichita", 37.6872m, -97.3301m },
                    { "Windsor", 42.3149m, -83.0364m },
                    { "Winnipeg", 49.8951m, -97.1384m },
                    { "Wolverhampton", 52.5862m, -2.1282m },
                    { "Wrocław", 51.1079m, 17.0385m },
                    { "Wuhan", 30.5928m, 114.3055m },
                    { "Xalapa", 19.5309m, -96.9155m },
                    { "Xi'an", 34.3416m, 108.9398m },
                    { "Yamunanagar", 30.1290m, 77.2674m },
                    { "Yangon", 16.8661m, 96.1951m },
                    { "Yonkers", 40.9312m, -73.8988m },
                    { "York", 53.9600m, -1.0873m },
                    { "Zagreb", 45.8150m, 15.9819m },
                    { "Zaragoza", 41.6488m, -0.8891m },
                    { "Zhengzhou", 34.7466m, 113.6254m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "A Coruña");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Abbotsford");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Aberdeen");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Acapulco");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Adoni");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Agartala");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Agra");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Aguascalientes");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ahmedabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ahmednagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Aizawl");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ajax");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ajmer");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Akola");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Akron");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Albuquerque");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Alexandria");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Aligarh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Allahabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Alwar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Amarillo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ambattur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ambernath");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Amravati");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Amritsar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Amroha");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Amsterdam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Anaheim");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Anantapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Anchorage");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ankara");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Antwerp");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Arlington");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Arrah");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Asansol");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Athens");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Atlanta");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Augusta");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Aurangabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Aurora");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Aurora IL");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Austin");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Avadi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Badalona");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Baghdad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bahadurgarh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bakersfield");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bally");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Baltimore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bangalore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bangkok");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Barabanki");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Baranagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bardhaman");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bareilly");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Barrie");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bathinda");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Baton Rouge");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Begusarai");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Beijing");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Belfast");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Belgaum");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bellary");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Belo Horizonte");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Berhampur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Berlin");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhagalpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhagha Purana");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bharatpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhatpara");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhavnagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhilai Nagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhilwara");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhiwandi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhopal");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bhubaneswar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bihar Sharif");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bijapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bikaner");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bilaspur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bilbao");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Birmingham");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Birmingham AL");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bogotá");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Boise");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bokaro");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bordeaux");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Boston");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bradford");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Brampton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Brasília");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bremen");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bristol");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Brussels");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Bucharest");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Budapest");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Buenos Aires");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Burlington");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Burnaby");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Buxar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cairo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Calgary");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cambridge");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cancún");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cardiff");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cartagena");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chandigarh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chandler");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chandrapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Changchun");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Changsha");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Charlotte");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chatham-Kent");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chengdu");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chennai");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chesapeake");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chicago");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chihuahua");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chimalhuacán");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chongqing");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Chula Vista");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cincinnati");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cleveland");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Coimbatore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cologne");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Colorado Springs");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Columbus");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Columbus GA");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Copenhagen");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Coquitlam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Córdoba");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Corpus Christi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Coventry");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cuernavaca");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Culiacán");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Cuttack");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dalian");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dallas");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dar es Salaam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Darbhanga");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Davanagere");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dehradun");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Delhi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Delta");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Denver");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Derby");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Des Moines");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Detroit");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dewas");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dhaka");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dhanbad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dhule");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dibrugarh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dieppe");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dongguan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dortmund");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dresden");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Dundee");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Durango");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Durg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Durgapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Durham");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Düsseldorf");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Edinburgh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Edmonton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "El Paso");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Elche");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Eluru");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "English Bazar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ensenada");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Erode");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Essen");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Etawah");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Faridabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Farrukhabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Fayetteville");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Firozabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Florence");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Fontana");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Fort Wayne");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Fort Worth");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Foshan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Frankfurt");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Fremont");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Fresno");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gandhidham");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Garland");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gatineau");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gaya");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gdańsk");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Genoa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ghaziabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gijón");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gilbert");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Giza");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Glasgow");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Glendale");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gopalpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gothenburg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Granada");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Grand Rapids");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Greensboro");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Guadalajara");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Guangzhou");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Guelph");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gulbarga");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Guntur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gurgaon");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Guwahati");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Gwalior");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Halifax");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hamburg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hamilton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hangzhou");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hannover");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hanoi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Harbin");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hefei");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Helsinki");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Henderson");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hermosillo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hialeah");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ho Chi Minh City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hong Kong");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Honolulu");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hospet");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Houston");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Howrah");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hubballi-Dharwad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Huntington Beach");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Huntsville");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Hyderabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ichalkaranji");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Imphal");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Indianapolis");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Indore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Irapuato");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Irvine");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Irving");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Istanbul");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jabalpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jacksonville");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jaipur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jakarta");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jalandhar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jalgaon");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jalna");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jammu");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jamnagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jamshedpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jehanabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jerez de la Frontera");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jersey City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jhansi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jinan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Jodhpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Johannesburg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Juárez");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Junagadh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kadapa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kakinada");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kalyan-Dombivali");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kamarhati");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kanpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kansas City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Karachi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Karimnagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Karnal");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Katihar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kelowna");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Khammam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Khartoum");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kinshasa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kirari Suleman Nagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kitchener");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kochi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kolhapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kolkata");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kollam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Korba");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kota");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kozhencherry");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kozhikode");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kraków");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Krishnanagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kuala Lumpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kulti");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kunming");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Kurnool");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "L'Hospitalet");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lagos");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lahore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Langley");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lanzhou");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Laredo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Las Palmas");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Las Vegas");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Las Venturas");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Latur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Laval");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Leeds");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Leicester");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Leipzig");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "León");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lévis");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lexington");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lille");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lima");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lincoln");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Little Rock");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Liverpool");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Łódź");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "London");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "London ON");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Long Beach");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Longueuil");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Loni");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Los Angeles");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Los Santos");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Louisville");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Luanda");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lubbock");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lucknow");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ludhiana");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Lyon");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Madison");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Madrid");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Madurai");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mahbubnagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Maheshtala");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Málaga");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Malegaon");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mamit");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Manchester");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mandsaur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mandurah");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mangalore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Manila");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Markham");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Marseille");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Matamoros");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mathura");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mau");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mazatlán");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Medellín");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Meerut");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Melbourne");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Memphis");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mérida");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mesa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mexicali");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mexico City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Miami");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Milan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Milton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Milwaukee");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Minneapolis");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mira-Bhayandar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mirzapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mississauga");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mobile");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Modesto");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Moncton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Monterrey");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Montgomery");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Montpellier");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Montreal");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Moradabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Morelia");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Moreno Valley");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Moscow");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Muktsar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mumbai");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Munich");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Murcia");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Muzaffarnagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Muzaffarpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Mysore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nagoya");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nagpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Naihati");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nanded");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nanjing");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nantes");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Naples");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nashik");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nashville");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Naucalpan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Navi Mumbai");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "New Delhi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "New Orleans");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "New York");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Newark");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Newcastle");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nice");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nizamabad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Noida");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Norfolk");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "North Dumdum");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "North Las Vegas");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Norwich");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nottingham");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nuevo Laredo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Nuremberg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oakland");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oakville");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oaxaca");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oklahoma City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Omaha");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Orlando");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Osaka");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oshawa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oslo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ottawa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oviedo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Oxnard");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ozhukarai");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Pachuca");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Palermo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Pali");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Palma");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Panihati");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Panipat");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Parbhani");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Paris");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Patiala");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Patna");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Peterborough");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Philadelphia");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Phoenix");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Pimpri-Chinchwad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Pittsburgh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Plano");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Plymouth");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Pondicherry");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Portland");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Portsmouth");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Poznań");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Prague");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Proddatur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Puebla");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Pune");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Purnia");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Puruliya");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Qingdao");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Quebec City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Querétaro");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Raichur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Raipur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rajahmundry");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rajkot");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rajpur Sonarpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Raleigh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ramagundam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rampur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ranchi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ratlam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Regina");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Reims");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rennes");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rewa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Reynosa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Richmond");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Richmond Hill");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rio de Janeiro");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Riverside");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Riyadh");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rochester");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rohtak");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rome");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rotterdam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Rourkela");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Saanich");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sacramento");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Saguenay");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Saharanpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Saint Paul");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Saint Petersburg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Salem");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Salford");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Salt Lake City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Saltillo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sambalpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "San Antonio");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "San Bernardino");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "San Diego");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "San Francisco");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "San Jose");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "San Luis Potosí");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sangli-Miraj & Kupwad");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Santa Ana");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Santiago");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "São Paulo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Saskatoon");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Satara");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Scottsdale");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Seattle");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Seoul");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Seville");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Shahjahanpur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Shanghai");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sheffield");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Shenyang");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Shenzhen");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sherbrooke");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Shijiazhuang");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Shivamogga");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Shreveport");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sikar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Silchar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Siliguri");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Singapore");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sofia");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Solapur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sonipat");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "South Dumdum");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Southampton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Spokane");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Srinagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "St. Catharines");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "St. John's");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "St. Louis");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "St. Petersburg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Stockholm");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Stockton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Stoke-on-Trent");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Strasbourg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Stuttgart");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sunderland");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Surat");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Surrey");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Suzhou");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Swansea");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Sydney");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tacoma");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Taiyuan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tallahassee");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tampa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tampico");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tehran");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tenali");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Terrassa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Terrebonne");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Thane");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Thanesar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "The Hague");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Thiruvananthapuram");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Thoothukudi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Thrissur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Thunder Bay");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tianjin");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tijuana");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tiruchirappalli");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tirunelveli");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tirupur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tiruvottiyur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tlalnepantla");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tlaquepaque");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tokyo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Toledo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Toluca");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Toronto");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Torreón");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Toulouse");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Trois-Rivières");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tucson");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tulsa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tumkur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Turin");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Tuxtla Gutiérrez");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Udaipur");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ujjain");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Ulhasnagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vadodara");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Valencia");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Valladolid");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vancouver");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Varanasi");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vasai-Virar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vaughan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Veracruz");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vice City");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vienna");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vigo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vijayawada");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Villahermosa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Virginia Beach");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Visakhapatnam");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Vitoria-Gasteiz");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Warangal");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Warsaw");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Washington");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Waterloo");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Westminster");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Whitby");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Wichita");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Windsor");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Winnipeg");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Wolverhampton");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Wrocław");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Wuhan");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Xalapa");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Xi'an");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Yamunanagar");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Yangon");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Yonkers");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "York");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Zagreb");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Zaragoza");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Name",
                keyValue: "Zhengzhou");
        }
    }
}
