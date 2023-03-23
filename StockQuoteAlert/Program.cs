using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;


namespace TesteAPI
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            string ativo = args[0];
            decimal precoRefVenda, precoRefCompra;

            if (args.Length != 3)
            {
                Console.WriteLine("Número incorreto de parâmetros. Você deve seguir o padrão: StockQuoteAlert.exe [código do ativo] [preço de venda] [preço de compra]");
                return;
            }

            if (!decimal.TryParse(args[1], out precoRefVenda) || !decimal.TryParse(args[2], out precoRefCompra))
            {
                Console.WriteLine("Os valores de preço devem ser numéricos!");
                return;
            }

            if (precoRefCompra > precoRefVenda)
            {
                Console.WriteLine("Os valores de preço passados não são coerentes. Não é estratégico o valor de compra ser maior que o de venda.");
                Console.WriteLine("Você deve seguir o padrão: StockQuoteAlert.exe [código do ativo] [preço de venda] [preço de compra]");
                return;
            }


            string QUERY_URL = "https://economia.awesomeapi.com.br/last/USD-BRL,EUR-BRL,BTC-BRL";
            //Opções de moeda para colocar na linha de comando para essa API de teste: USD,EUR,BTC
            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    //fazendo requisição para a API
                    HttpResponseMessage respostaAPI = await client.GetAsync(QUERY_URL);
                    string json_string = await respostaAPI.Content.ReadAsStringAsync();
                    Dictionary<string, JsonElement> json_data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json_string);



                    try
                    {

                        JsonElement ativoElement = json_data[ativo + "BRL"];

                        JsonElement CompraElement = ativoElement.GetProperty("bid");
                        string PrecoCompra = CompraElement.GetString();

                        JsonElement VendaElement = ativoElement.GetProperty("ask");
                        string PrecoVenda = VendaElement.GetString();


                        decimal PrecoCompraFinal = Convert.ToDecimal(PrecoCompra) / 10000;
                        decimal PrecoVendaFinal = Convert.ToDecimal(PrecoVenda) / 10000;


                        Console.WriteLine($"A cotação da ação {ativo} é: R$ {PrecoCompraFinal.ToString("N2")}");
                        //Console.WriteLine($"A cotação da ação {ativo} é: R$ {PrecoVendaFinal.ToString("N2")}");


                        //LER ARQUIVO DE CONFIGURAÇÃO (para email)
                        var destinatario = ConfigurationManager.AppSettings["destinatario"];
                        var remetente = ConfigurationManager.AppSettings["remetente"];
                        var senha = ConfigurationManager.AppSettings["senha"];
                        var host = ConfigurationManager.AppSettings["host"];
                        int port = Int32.Parse(ConfigurationManager.AppSettings["port"]);

                        //Para testes de disparo de email
                        /*Console.Write("Digite um valor: ");
                        string input = Console.ReadLine();
                        int cotacao = int.Parse(input);*/

                        string body, subject;
                        if (PrecoCompraFinal < precoRefCompra)
                        {
                            subject = "Alerta de compra teste INOA";
                            body = "Recomendamos que você compre o ativo " + ativo;

                            //ENVIAR O EMAIL COMPRA
                            try
                            {

                                MailMessage mailMessage1 = new MailMessage(remetente, destinatario);

                                mailMessage1.Subject = subject;
                                mailMessage1.Body = body;
                                mailMessage1.SubjectEncoding = Encoding.GetEncoding("UTF-8"); //reconhece acentuações no assunto do email
                                mailMessage1.BodyEncoding = Encoding.GetEncoding("UTF-8"); //reconhece acentuações no corpo do email

                                SmtpClient smtpClient1 = new SmtpClient(host, port);

                                smtpClient1.UseDefaultCredentials = false;
                                smtpClient1.Credentials = new NetworkCredential(remetente, senha);

                                smtpClient1.EnableSsl = true;

                                smtpClient1.Send(mailMessage1);

                                Console.WriteLine("Email de compra enviado");
                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine("Houve um erro no envio do email de compra.");
                                Console.WriteLine(ex.Message);
                            }

                        }
                        else if (PrecoVendaFinal > precoRefVenda)
                        {
                            subject = "Alerta de venda teste INOA";
                            body = "Recomendamos que você venda o ativo " + ativo;

                            //ENVIAR O EMAIL VENDA
                            try
                            {

                                MailMessage mailMessage2 = new MailMessage(remetente, destinatario);

                                mailMessage2.Subject = subject;
                                mailMessage2.Body = body;
                                mailMessage2.SubjectEncoding = Encoding.GetEncoding("UTF-8"); //reconhece acentuações no assunto do email
                                mailMessage2.BodyEncoding = Encoding.GetEncoding("UTF-8"); //reconhece acentuações no corpo do email

                                SmtpClient smtpClient2 = new SmtpClient(host, port);

                                smtpClient2.UseDefaultCredentials = false;
                                smtpClient2.Credentials = new NetworkCredential(remetente, senha);

                                smtpClient2.EnableSsl = true;

                                smtpClient2.Send(mailMessage2);

                                Console.WriteLine("Email de venda enviado");
                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine("Houve um erro no envio do email de venda.");
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("nao manda email");
                        }


                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    await Task.Delay(30000);
                }
            }
        }
    }
}