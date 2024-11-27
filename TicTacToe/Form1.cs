using System;
using System.Drawing;
using System.Windows.Forms;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        private string currentPlayer = "X";
        private Server server;
        private Client client;

        public Form1()
        {
            InitializeComponent();
            InitializeGameBoard();
            InitializeNetworkButtons();
        }

        private void InitializeGameBoard()
        {
            buttons = new Button[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    buttons[i, j] = new Button
                    {
                        Location = new Point(50 + 100 * i, 50 + 100 * j),
                        Size = new Size(90, 90),
                        Font = new Font("Microsoft Sans Serif", 24),
                        Tag = new Point(i, j)
                    };
                    buttons[i, j].Click += Button_Click;
                    Controls.Add(buttons[i, j]);
                }
            }
        }

        private void InitializeNetworkButtons()
        {
            // Adaugă butoane pentru inițializarea serverului și conectarea clientului
            Button serverButton = new Button
            {
                Text = "Start Server",
                Location = new Point(50, 400),
                Size = new Size(100, 30)
            };
            serverButton.Click += (sender, e) => StartServer();
            Controls.Add(serverButton);

            Button clientButton = new Button
            {
                Text = "Connect as Client",
                Location = new Point(200, 400),
                Size = new Size(150, 30)
            };
            clientButton.Click += (sender, e) =>
            {
                string ipAddress = Prompt.ShowDialog("Enter Server IP", "Connect as Client");
                ConnectToServer(ipAddress);
            };
            Controls.Add(clientButton);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if (button.Text == "")
            {
                button.Text = currentPlayer;
                Point location = (Point)button.Tag;
                string message = $"{location.X},{location.Y},{currentPlayer}";

                // Trimite mesajul către server sau client
                if (server != null)
                {
                    server.SendMessage(message);
                }
                else if (client != null)
                {
                    client.SendMessage(message);
                }

                if (CheckForWinner())
                {
                    messageLabel.Text = $"{currentPlayer} a câștigat!";
                    DisableButtons();
                }
                else
                {
                    SwitchPlayer();
                }
            }
        }

        private void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == "X") ? "O" : "X";
            messageLabel.Text = $"Este rândul lui {currentPlayer}";
        }

        private bool CheckForWinner()
        {
            // Verifică rândurile, coloanele și diagonalele pentru un câștigător
            for (int i = 0; i < 3; i++)
            {
                if (buttons[i, 0].Text != "" &&
                    buttons[i, 0].Text == buttons[i, 1].Text &&
                    buttons[i, 1].Text == buttons[i, 2].Text)
                {
                    return true;
                }

                if (buttons[0, i].Text != "" &&
                    buttons[0, i].Text == buttons[1, i].Text &&
                    buttons[1, i].Text == buttons[2, i].Text)
                {
                    return true;
                }
            }

            if (buttons[0, 0].Text != "" &&
                buttons[0, 0].Text == buttons[1, 1].Text &&
                buttons[1, 1].Text == buttons[2, 2].Text)
            {
                return true;
            }

            if (buttons[0, 2].Text != "" &&
                buttons[0, 2].Text == buttons[1, 1].Text &&
                buttons[1, 1].Text == buttons[2, 0].Text)
            {
                return true;
            }

            return false;
        }

        private void DisableButtons()
        {
            foreach (Button btn in buttons)
            {
                btn.Enabled = false;
            }
        }

        private void StartServer()
        {
            server = new Server();
            server.OnMessageReceived += Server_OnMessageReceived;
        }

        private void ConnectToServer(string ipAddress)
        {
            client = new Client(ipAddress);
            client.OnMessageReceived += Client_OnMessageReceived;
        }

        private void Server_OnMessageReceived(string message)
        {
            // Actualizează interfața grafică pe baza mesajului primit
            this.Invoke((MethodInvoker)delegate
            {
                HandleMove(message);
            });
        }

        private void Client_OnMessageReceived(string message)
        {
            // Actualizează interfața grafică pe baza mesajului primit
            this.Invoke((MethodInvoker)delegate
            {
                HandleMove(message);
            });
        }

        private void HandleMove(string message)
        {
            // Parsează și actualizează tabla de joc pe baza mesajului primit
            var parts = message.Split(',');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            string player = parts[2];

            buttons[x, y].Text = player;
            if (CheckForWinner())
            {
                messageLabel.Text = $"{player} a câștigat!";
                DisableButtons();
            }
            else
            {
                SwitchPlayer();
            }
        }
    }
}
