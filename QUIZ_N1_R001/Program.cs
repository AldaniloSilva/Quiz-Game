/*
 * Created by SharpDevelop.
 * User: alsilva
 * Date: 07/03/2018
 * Time: 09:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace QUIZ_N1
{
    class Program
    {
        static Stopwatch cronometro = new Stopwatch();
        static int tempoemSegundos;//variavel que armazena o tempo em segundos
        const int tr = 8;//variavel que define o tempo para responder as perguntas
        struct Questionario
        {
            public string pergunta;
            public string tema;
            public string correta;
            public string escolhida;
            public string[] alternativa;

        }
        /// <summary>
        /// DISTRIBUI O TEXTO NA ESTRUTURA
        /// </summary>
        /// <param name="trecho"></param>
        /// <returns></returns>
        #region MÉTODO SEPARA O TEXTO NA ESTRUTURA QUESTIONÁRIO
        static string[] SeparaPalavra(string trecho)
        {
            //Esse método recebe a string trecho com todo o texto do arquivo
            //E devolve um vetor de duas posições uma com a palavra a ser guardada 
            //e a outra com o novo trecho a ser cortado
            int pos;//variavél que indica posição
            string novotrecho = "";//indica o novo trecho após o "corte"
            trecho = trecho.Trim();
            pos = trecho.IndexOf('|');
            if (pos == -1)
                pos = trecho.Length;
            string palavra = trecho.Substring(0, pos);
            palavra = palavra.Trim();
            if (trecho.IndexOf('|') != -1)
                novotrecho = trecho.Remove(0, pos + 1);
            string[] retorno = { palavra, novotrecho };
            return retorno;
        }
        #endregion

        static void Main(string[] args)
        {
            if (File.Exists("QUIZ.txt"))

            {
                string[] texto = File.ReadAllLines("QUIZ.txt", Encoding.UTF8);
                Questionario[] linha = new Questionario[texto.Length];//Vetor linha guardará todas as informações de cada linha
                string[] novoformato = new string[2];//Vetor que receberá a palavra separada do resto do texto
                for (int n = 0; n < texto.Length; n++)
                {
                    linha[n] = new Questionario();

                    //Separa pergunta
                    novoformato = SeparaPalavra(texto[n]);
                    linha[n].pergunta = novoformato[0];


                    //Separa tema
                    novoformato = SeparaPalavra(novoformato[1]);
                    linha[n].tema = novoformato[0];

                    //Separa alternativa correta
                    novoformato = SeparaPalavra(novoformato[1]);
                    linha[n].correta = novoformato[0];

                    //Separa as 4 alternativas
                    string[] vetorpalavra = new string[4];
                    for (int a = 0; a < vetorpalavra.Length; a++)
                    {
                        linha[n].alternativa = vetorpalavra;
                        novoformato = SeparaPalavra(novoformato[1]);
                        linha[n].alternativa[a] = novoformato[0];
                    }
                }
                //Após processar o arquivo txt a rotina do jogo começa aqui
                Console.WriteLine("----------------QUIZ--------------------");

                string[] tema = SelecionaTema(texto.Length, linha);
                for (int n = 0; n < tema.Length; n++)
                {
                    //Console.WriteLine($"{n + 1} - {tema[n]}");
                    Console.WriteLine("{0} - {1}", n + 1, tema[n]);
                }
                int resp = LeOpcao("\nDigite o tema desejado: ", tema.Length);//variavél resp recebe a informação da opção escolhida
                int qtpergunta = ContaPergunta(tema[resp - 1], texto.Length, linha);//variavél recebe a quantiade de pergunta de um tema 

                Console.WriteLine("Há disponível {0} perguntas sobre esse tema", qtpergunta);
                int peg = LeOpcao("\nDigite a quantidade de perguntas desse tema: ", qtpergunta);



                //No final da execução da próxima linha as posições das perguntas já estão misturadas
                //Dentro do vetor PerguntasProntas
                int[] PerguntasProntas = SeparaQuestao(tema[resp - 1], texto.Length, linha, peg, qtpergunta);

                //Na próxima linha as perguntas serão apresentadas na tela
                ApresentaPergunta(PerguntasProntas, peg, linha);


                int corretas = ContagemdePontos(PerguntasProntas, peg, linha);//variavél recebe a quantidade de perguntas corretas

                ApresentaResumo(PerguntasProntas, peg, corretas, linha);



            }

            else
                Console.WriteLine("O jogo não pode ser rodado pois o arquivo QUIZ.txt não existe!");
            Console.ReadLine();
        }


        /// <summary>
        /// ESSE MÉTODO IRÁ SEPARAR TODOS OS TEMAS EM UM ÚNICO VETOR
        /// </summary>
        /// <param name="texto"></param>
        /// <param name="nomeTema"></param>
        /// <returns></returns>
        #region MÉTODO SELECIONA TEMA
        static string[] SelecionaTema(int texto, Questionario[] nomeTema)//Variavél texto indica quantidade de linhas do arquivo QUIZ.txt
        {

            string tipo = "";
            for (int n = 0; n < texto; n++)
            {
                if (!tipo.Contains(nomeTema[n].tema))
                    tipo = tipo + nomeTema[n].tema + ';';
            }
            tipo = tipo.Substring(0, tipo.Length - 1);
            string[] vetorTipo = tipo.Split(';');
            return vetorTipo;
        }
        #endregion

        ///<summary>
        /// AQUI SERÁ LIDO A OPÇÃO DO USUÁRIO SOBRE O TEMA ESCOLHIDO E QTD DE PERGUNTAS PARA RESPONDER
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="qt"></param>
        /// <returns></returns>
        #region MÉTODO LÊ OPÇÃO
        static int LeOpcao(string msg, int qt)
        {
            int numero;
            string valorLido;
            do
            {
                do
                {
                    Console.Write(msg);
                    valorLido = Console.ReadLine();
                } while (int.TryParse(valorLido, out numero) == false);
            } while (numero <= 0 || numero > qt);
            return numero;


        }
        #endregion


        /// <summary>
        /// CONTA QUANTAS PERGUNTAS TEM O TEMA ESCOLHIDO
        /// </summary>
        /// <param name="tema"></param>
        /// <param name="totalPergunta"></param>
        /// <param name="PerguntaComTema"></param>
        /// <returns></returns>
        #region APRESENTA A QUANTIDADE DE PERGUNTAS DO TEMA ESCOLHIDO
        static int ContaPergunta(string tema, int totalPergunta, Questionario[] PerguntaComTema)
        {
            int conta = 0;
            for (int n = 0; n < totalPergunta; n++)
            {
                if (PerguntaComTema[n].tema == tema)
                    conta = conta + 1;
            }
            return conta;

        }
        #endregion


        /// <summary>
        /// QUESTÕES DO MESMO TEMA SÃO REUNIDAS NO MESMO VETOR ANTES DE MISTURAR
        /// </summary>
        /// <param name="tema"></param>
        /// <param name="texto"></param>
        /// <param name="tipo"></param>
        /// <param name="peg"></param>
        /// <param name="qtquestao"></param>
        /// <returns></returns>
        #region RETORNA AS POSIÇÕES DAS PERGUNTAS EM UM VETOR
        static int[] SeparaQuestao(string tema, int texto, Questionario[] tipo, int peg, int qtquestao)
        {
            int c = 0;
            int[] vetorPergunta = new int[qtquestao];
            for (int n = 0; n < texto; n++)
            {
                if (tema == tipo[n].tema)
                {
                    vetorPergunta[c] = n;
                    c = c + 1;
                }
                if (c == qtquestao)
                    break;

            }
            //O método Mistua Questão está levando para esse método o vetorPergunta que tem
            //todas as posições da pergunta escolhida e a quantiadade escolhida
            int[] PerguntasProntas = (MisturaQuestao(vetorPergunta, peg));//Nessa etapa as perguntas já estão selecionadas e prontas para serem exibidas


            return PerguntasProntas;

        }
        #endregion


        /// <summary>
        /// ESSE MÉTODO IRÁ RETORNAR AS POSIÇÕES DAS PERGUNTAS DE UM MESMO TEMA TODAS MISTURADAS
        /// </summary>
        /// <param name="vetorPergunta"></param>
        /// <param name="peg"></param>
        /// <returns></returns>
        #region MISTURA AS POSIÇÕES DAS PERGUNTAS E ENVIA PARA O MÉTODO SEPARA QUESTÃO
        static int[] MisturaQuestao(int[] vetorPergunta, int peg)
        {
            Random sorteio = new Random();
            int[] VetorMisturado = new int[peg];
            VetorMisturado[0] = (sorteio.Next(0, vetorPergunta.Length));
            for (int n = 1; n < peg; n++)
            {
                VetorMisturado[n] = (sorteio.Next(0, vetorPergunta.Length));
                for (int c = 0; c < n; c++)
                {
                    while (VetorMisturado[n] == VetorMisturado[c])
                    {
                        VetorMisturado[n] = (sorteio.Next(0, vetorPergunta.Length));
                        c = 0;
                    }
                }

            }
            for (int n = 0; n < peg; n++)
            {
                VetorMisturado[n] = vetorPergunta[VetorMisturado[n]];

            }
            return VetorMisturado;
        }
        #endregion

        /// <summary>
        /// MÉTODO RESPONSAVÉL POR MOSTRAR AS PERGUNTAS NA TELA
        /// </summary>
        /// <param name="PerguntasProntas"></param>
        /// <param name="peg"></param>
        /// <param name="linha"></param>
        #region APRESENTA AS PERGUNTAS NO VIDEO, SEPARA A ALTERNATIVA ESCOLHIDA, E COMPARA COM A CORRETA
        static void ApresentaPergunta(int[] PerguntasProntas, int peg, Questionario[] linha)
        {
            for (int n = 0; n < peg; n++)
            {

                Console.Clear();
                Console.WriteLine(linha[PerguntasProntas[n]].pergunta);
                for (int a = 0; a < 4; a++)//a maior que 4 pois só temos 4 alternativas
                {
                    Console.WriteLine("{0} -  {1}", a + 1, linha[PerguntasProntas[n]].alternativa[a]);

                }

                int alter = ControleTempo(peg);
                if (alter != 0)
                {
                    linha[PerguntasProntas[n]].escolhida = linha[PerguntasProntas[n]].alternativa[alter - 1];
                    Console.WriteLine();
                }
                else
                    linha[PerguntasProntas[n]].escolhida = "";
                if (tempoemSegundos >= (peg * tr))
                    break;
            }
            cronometro.Stop();
            return;
        }
        #endregion


        /// <summary>
        /// FAZ A CONTAGEM DE PERGUNTAS CORRETAS
        /// </summary>
        /// <param name="PerguntasProntas"></param>
        /// <param name="peg"></param>
        /// <param name="linha"></param>
        /// <returns></returns>
        #region CONTA QUANTAS PERGUNTAS ESTÃO CORRETAS
        static int ContagemdePontos(int[] PerguntasProntas, int peg, Questionario[] linha)
        {
            int correta = 0;
            for (int n = 0; n < peg; n++)
            {
                if (linha[PerguntasProntas[n]].escolhida == linha[PerguntasProntas[n]].correta)
                    correta = correta + 1;
            }

            return correta;
        }
        #endregion



        /// <summary>
        /// MOSTRA O RESUMO DO JOGO E PONTUAÇÃO
        /// </summary>
        /// <param name="PerguntasProntas"></param>
        /// <param name="peg"></param>
        /// <param name="correta"></param>
        /// <param name="linha"></param>
        #region RESUMO DO JOGO
        static void ApresentaResumo(int[] PerguntasProntas, int peg, int correta, Questionario[] linha)
        {
            Console.Clear();
            Console.WriteLine("----------------RESUMO GERAL-------------------");

            for (int n = 0; n < peg; n++)
            {

                Console.WriteLine();
                Console.WriteLine(linha[PerguntasProntas[n]].pergunta);
                Console.WriteLine("Reposta Correta: {0}", linha[PerguntasProntas[n]].correta);
                if (linha[PerguntasProntas[n]].correta == linha[PerguntasProntas[n]].escolhida)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Respondida: {0} ", linha[PerguntasProntas[n]].escolhida);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Respondida: {0} ", linha[PerguntasProntas[n]].escolhida);
                    Console.ResetColor();

                }
                Console.WriteLine();
            }
            Console.WriteLine("Você acertou {0} pergunta(s)", correta);
            Console.WriteLine("Você errou {0} pergunta(s)", peg - correta);
            return;
        }
        #endregion




        /// <summary>
        /// MÉTODO PRINCIPAL PARA O CRONOMETRO
        /// </summary>
        /// <param name="qtquestao"></param>
        /// <returns></returns>
        #region MÉTODO PRINCIPAL PARA A CONTAGEM DO TEMPO
        static int ControleTempo(int qtquestao)
        {
            int tempo = qtquestao * tr;
            bool variavelpreenchida = false;
            int numero = 0;

            cronometro.Start();

            ConsoleKeyInfo tecla;
            do
            {
                Console.CursorLeft = 0;
                Console.CursorTop = 9;
                tempoemSegundos = Convert.ToInt32(cronometro.Elapsed.TotalSeconds);
                Console.WriteLine("TEMPO: Ainda restam " + (tempo - tempoemSegundos + "seg."));
                Thread.Sleep(100);

                Console.CursorLeft = 0;
                Console.CursorTop = 7;
                Console.Write("Escolha uma das alternativas: ");

                LoopingRelogio(tempo);

                Thread.Sleep(250);
                if ((tempo - tempoemSegundos) <= 0)
                    break;

                tecla = Console.ReadKey(true);
                LeituraTecla(tecla, ref numero);
                if (numero != 0)
                    variavelpreenchida = true;

            } while (!variavelpreenchida);
            return numero;
        }
        #endregion


        /// <summary>
        /// POSICIONA NA TECLA A LINHA DA MENSAGEM ESCOLHA ALTERNATIVA
        /// </summary>
        /// <param name="numeros"></param>
        /// <returns></returns>
        #region
        static int MostraTela(int numeros)
        {
            Console.CursorLeft = 18;
            Console.CursorTop = 7;
            Console.Write("\rEscolha uma das alternativas: {0}", numeros);
            return numeros;
        }
        #endregion


        /// <summary>
        /// ESSE MÉTODO ENTRA EM UM LOOPING PARA QUE SEJA MOSTRADO A CONTAGEM REGRESSIVA
        /// </summary>
        /// <param name="tempo"></param>
        /// <returns></returns>
        #region FAZ OS CALCULOS DA CONTAGEM REGRESSIVA
        static int LoopingRelogio(int tempo)
        {
            while (Console.KeyAvailable == false)
            {

                Console.CursorLeft = 0;
                Console.CursorTop = 9;
                tempoemSegundos = Convert.ToInt32(cronometro.Elapsed.TotalSeconds);
                Console.WriteLine("TEMPO: Ainda restam " + (tempo - tempoemSegundos + "seg."));
                Thread.Sleep(100);


                if ((tempo - tempoemSegundos) <= 0)
                    break;
            }
            return tempo;
        }
        #endregion

        /// <summary>
        /// VERIFICA QUAL TECLA FOI PRESSIONADA
        /// QUALQUER TECLA DIFERENTE DE 1, 2, 3 OU 4 SERÁ IGNORADA
        /// </summary>
        /// <param name="tecla"></param>
        /// <param name="numero"></param>
        /// <returns></returns>
        #region LÊ QUAL TECLA ESTÁ SENDO PRESSIONADA
        static int LeituraTecla(ConsoleKeyInfo tecla, ref int numero)
        {
            bool variavelpreenchida = false;
            do
            {
                if (tecla.Key == ConsoleKey.D4 || tecla.Key == ConsoleKey.NumPad4)
                {//tecla 4 para alternativa 4
                    numero = 4;
                    MostraTela(numero);
                    Thread.Sleep(100);
                    variavelpreenchida = true;

                }

                else if (tecla.Key == ConsoleKey.D3 || tecla.Key == ConsoleKey.NumPad3)
                {//tecla 3 para alternativa 3
                    numero = 3;
                    MostraTela(numero);
                    Thread.Sleep(100);
                    variavelpreenchida = true;
                }

                else if (tecla.Key == ConsoleKey.D2 || tecla.Key == ConsoleKey.NumPad2)
                {//tecla 2 para alternativa 2
                    numero = 2;
                    MostraTela(numero);
                    Thread.Sleep(100);
                    variavelpreenchida = true;
                }

                else if (tecla.Key == ConsoleKey.D1 || tecla.Key == ConsoleKey.NumPad1)
                {//tecla 1 para alternativa 1
                    numero = 1;
                    MostraTela(numero);
                    Thread.Sleep(100);
                    variavelpreenchida = true;
                }
                else
                {//qualquer outra tecla sairá desse método
                    Console.CursorLeft = 18;
                    Console.CursorTop = 7;
                    Console.Write("\rEscolha uma das alternativas:   ");
                    Thread.Sleep(100);
                    return numero;
                }
            } while (!variavelpreenchida);
            return numero;
        }
        #endregion

    }





}

