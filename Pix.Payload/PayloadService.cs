using System;
using System.Text;

namespace Pix.Payload
{
    public class PayloadService
    {
        private readonly string ID_PAYLOAD_FORMAT_INDICATOR = "00";
    private readonly string ID_MERCHANT_ACCOUNT_INFORMATION = "26";
    private readonly string ID_MERCHANT_ACCOUNT_INFORMATION_GUI = "00";
    private readonly string ID_MERCHANT_ACCOUNT_INFORMATION_KEY = "01";
    private readonly string ID_MERCHANT_ACCOUNT_INFORMATION_DESCRIPTION = "02";
    private readonly string ID_MERCHANT_CATEGORY_CODE = "52";
    private readonly string ID_TRANSACTION_CURRENCY = "53";
    private readonly string ID_TRANSACTION_AMOUNT = "54";
    private readonly string ID_COUNTRY_CODE = "58";
    private readonly string ID_MERCHANT_NAME = "59";
    private readonly string ID_MERCHANT_CITY = "60";
    private readonly string ID_ADDITIONAL_DATA_FIELD_TEMPLATE = "62";
    private readonly string ID_ADDITIONAL_DATA_FIELD_TEMPLATE_TXID = "05";
    private readonly string ID_CRC16 = "63";

    public PayloadService(string pixKey, string paymentDescription, string merchantName, string merchantCity, string txId, string amount)
    {
        PixKey = pixKey;
        PaymentDescription = paymentDescription;
        MerchantName = merchantName;
        MerchantCity = merchantCity;
        TxId = txId;
        Amount = amount;
    }

    // CHAVE PIX
    public string PixKey { get; set; }
    // Descrição do pagamento
    public string PaymentDescription { get; set; }
    // Nome do titular da conta
    public string MerchantName { get; set; }
    // Cidade do titular da conta
    public string MerchantCity { get; set; }
    // Id da transação pix
    public string TxId { get; set; }
    // Valor da transação pix
    public string Amount { get; set; }


    /*
    * Método responsável por retornar um valor completo do payload
    */
    private string GetValue(string id, string value){
        var size = value.Length.ToString().PadLeft(2, '0');
        return id + size + value;
    }

    /*
    * Método responsável por retornar os valores da conta
    */
    private string GetMerchantAccountInformation(){
        // Dominio do banco
        var gui = this.GetValue(this.ID_MERCHANT_ACCOUNT_INFORMATION_GUI, "br.gov.bcb.pix");
        // Chave pix
        var key = this.GetValue(this.ID_MERCHANT_ACCOUNT_INFORMATION_KEY, this.PixKey);
        // Descrição do pagamento
        var description = !string.IsNullOrEmpty(this.PaymentDescription) ? this.GetValue(this.ID_MERCHANT_ACCOUNT_INFORMATION_DESCRIPTION, this.PaymentDescription) : "";

        // Dados completos da conta
        var completData = this.GetValue(this.ID_MERCHANT_ACCOUNT_INFORMATION, gui + key + description);

        return completData;
    }

    /*
    * Método responsável por retornar os valores completos do campo adicional do Pix (TxId)
    */
    private string GetAdditionalDataFieldTemplate(){
        var txId = this.GetValue(this.ID_ADDITIONAL_DATA_FIELD_TEMPLATE_TXID, this.TxId);

        return this.GetValue(this.ID_ADDITIONAL_DATA_FIELD_TEMPLATE, txId);
    }

    /*
    * Método responsável por calcular o valor da hash de validação do código pix
    */
    private string GetCRC16(String payload) {
        //ADICIONA DADOS GERAIS NO PAYLOAD
        payload += this.ID_CRC16 + "04";
  
        //DADOS DEFINIDOS PELO BACEN
        var polinomio = 0x1021;
        var resultado = 0xFFFF;
  
        byte[] bytes = Encoding.Default.GetBytes(payload);

        //CHECKSUM
        if (bytes.Length > 0) {
            for (var i = 0; i < bytes.Length; i++) {
              resultado ^= (Convert.ToInt32(bytes[i]) << 8);
              for (var bitwise = 0; bitwise < 8; bitwise++) {
                if (((resultado <<= 1) & 0x10000) > 0){
                    resultado ^= polinomio;
                }
                resultado &= 0xffff;
              }
            }
          }
  
        //RETORNA CÓDIGO CRC16 DE 4 CARACTERES
        return this.ID_CRC16 + "04" + resultado.ToString("X").ToUpper();
    }

    
    /*
    * Método responsável por gerar o código completo do payload Pix
    */
    public string GetPayload(){
        var payload = this.GetValue(this.ID_PAYLOAD_FORMAT_INDICATOR, "01")
                                    + this.GetMerchantAccountInformation()
                                    + this.GetValue(this.ID_MERCHANT_CATEGORY_CODE, "0000")
                                    + this.GetValue(this.ID_TRANSACTION_CURRENCY, "986")
                                    + this.GetValue(this.ID_TRANSACTION_AMOUNT, this.Amount)
                                    + this.GetValue(this.ID_COUNTRY_CODE, "BR")
                                    + this.GetValue(this.ID_MERCHANT_NAME, this.MerchantName)
                                    + this.GetValue(this.ID_MERCHANT_CITY, this.MerchantCity)
                                    + this.GetAdditionalDataFieldTemplate();

        return payload + this.GetCRC16(payload);
    }
    }
}
