namespace ApiControleFinanceiro.Json
{
    public class Convert
    {
        public static string Serialize(Object o)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(o);
        }
        public static T Deserialize<T>(string jSon)
        {
            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jSon)!;
        }

    }
}
