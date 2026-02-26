using System.Xml.Linq;
using userPanelOMR.Context;
using userPanelOMR.model;

namespace userPanelOMR.Service
{
    public class HashEncption 
    {
        public string ComputeSha256Hash(string rawData)
        {
            using (var sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                var builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
