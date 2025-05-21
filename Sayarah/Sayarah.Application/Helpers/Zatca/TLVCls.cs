/***
 * This code was built according to the specifications provided by “Zakat, Tax and Customs Authority”
 * URL： https://zatca.gov.sa/en/E-Invoicing/SystemsDevelopers/Documents/QRCodeCreation.pdf
 */

using System;
using System.Text;
using System.Collections.Generic;



namespace Sayarah.Application.Helpers.Zatca
{
    public class TLVCls
    {
        byte[] Seller;
        byte[] VatNo;
        byte[] dateTime;
        byte[] Total;
        byte[] Tax;

        public TLVCls(string Seller, string TaxNo, DateTime dateTime, double Total, double Tax)
        {
            this.Seller = Encoding.UTF8.GetBytes(Seller);
            VatNo = Encoding.UTF8.GetBytes(TaxNo);

            this.dateTime = Encoding.UTF8.GetBytes(dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
            this.Total = Encoding.UTF8.GetBytes(Total.ToString());
            this.Tax = Encoding.UTF8.GetBytes(Tax.ToString());
        }

        private string getasText(int Tag, byte[] Value)
        {
            return Tag.ToString("X2") + Value.Length.ToString("X2") + BitConverter.ToString(Value).Replace("-", string.Empty);
        }

        private byte[] getBytes(int id, byte[] Value)
        {
            byte[] val = new byte[2 + Value.Length];
            val[0] = (byte)id;
            val[1] = (byte)Value.Length;
            Value.CopyTo(val, 2);
            return val;
        }

        private string getString()
        {
            string TLV_Text = "";
            TLV_Text += getasText(1, Seller);
            TLV_Text += getasText(2, VatNo);
            TLV_Text += getasText(3, dateTime);
            TLV_Text += getasText(4, Total);
            TLV_Text += getasText(5, Tax);
            return TLV_Text;
        }

        public override string ToString()
        {
            return getString();
        }

        public string ToBase64()
        {
            List<byte> TLV_Bytes = new List<byte>();
            TLV_Bytes.AddRange(getBytes(1, Seller));
            TLV_Bytes.AddRange(getBytes(2, VatNo));
            TLV_Bytes.AddRange(getBytes(3, dateTime));
            TLV_Bytes.AddRange(getBytes(4, Total));
            TLV_Bytes.AddRange(getBytes(5, Tax));
            return Convert.ToBase64String(TLV_Bytes.ToArray());
        }

        //public Bitmap toQrCode(int width=250, int height = 250)
        //{

        //    BarcodeWriter barcodeWriter = new BarcodeWriter {
        //        Format = BarcodeFormat.QR_CODE,
        //        Options = new EncodingOptions {
        //            Width = width,
        //            Height = height
        //        } };
        //    Bitmap QrCode = barcodeWriter.Write(this.ToBase64());

        //    return QrCode;
        //}


    }

}
