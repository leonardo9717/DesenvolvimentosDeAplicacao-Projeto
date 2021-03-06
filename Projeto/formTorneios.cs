﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto {
    public partial class formTorneios : Form {

        private Modelo_Container dbContainer;

        private TournamentRepository tourRepo;
        private GameRepository gameRepo;
        private ArbitroRepository arbitroRepo;
        private DeckRepository deckRepo;
        private PlayerRepository playerRepo;
        private EquipasRepository equipaRepo;

        private List<Referee> listaArbitros;
        private List<Deck> listaDecks1;
        private List<Deck> listaDecks2;
        private List<Player> listaPlayers1;
        private List<Player> listaPlayers2;
        private List<Team> listaTeams1;
        private List<Team> listaTeams2;
        private List<StandardGame> listaStandGames;
        private List<TeamGame> listaTeamGames;
        private List<StandardToutnament> listaStandardTourn;
        private List<TeamTournament> listaTeamTourn;

        private bool flagEditarTorneio = false;
        private bool flagEditarJogo = false;
        private bool flagLoginArbitro = false;
        private bool flagLoggingOut = false;

        private Referee arbitro;
        private formLogin formlogin;

        public formTorneios(Modelo_Container dbContainer)
        {
            InitializeComponent();
            this.dbContainer = dbContainer;
            Init();
            menuStrip1.Visible = false;
        }
        public formTorneios(Modelo_Container dbContainer, Referee arbitro, formLogin formLogin) {
            InitializeComponent();
            this.dbContainer = dbContainer;
            Init();
            this.arbitro = arbitro;
            this.formlogin = formLogin;
            flagLoginArbitro = true;
            btNovoTorn.Enabled = false;
            menuStrip1.Visible = true;
        }

        private void Init() {
            tourRepo = new TournamentRepository(dbContainer);
            gameRepo = new GameRepository(dbContainer);
            arbitroRepo = new ArbitroRepository(dbContainer);
            deckRepo = new DeckRepository(dbContainer);
            playerRepo = new PlayerRepository(dbContainer);
            equipaRepo = new EquipasRepository(dbContainer);
            RefreshListTorneiosNormais();
            RefreshListTorneiosEquipas();
        }
        /************************************************************************************/
        /*                               TORNEIOS                                           */
        /************************************************************************************/

        /********************************* EVENTOS ******************************************/
        private void formTorneios_FormClosing(object sender, FormClosingEventArgs e) {
            if (flagLoginArbitro && !flagLoggingOut)
                Application.Exit();
        }

        private void btNovoTorn_Click(object sender, EventArgs e) {
            flagEditarTorneio = false;
            AtivarFormTorneio(true);
            AtivarFormJogo(false);
            LimpaFormJogos();
            btNovoTorn.Enabled = false;
            btNovoJog.Enabled = false;
            LimpaFormTorneio();
        }

        private void btRemoverTorn_Click(object sender, EventArgs e) {
            if (flagEditarTorneio)  {//MODO EDIÇAO
                if (rbNormal.Checked) {
                    tourRepo.DeleteTournament(listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex));
                    RefreshListTorneiosNormais();
                } else {
                    tourRepo.DeleteTournament(listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex));
                    RefreshListTorneiosEquipas();
                }
            }
            LimpaFormTorneio();
            AtivarFormTorneio(false);
            btNovoTorn.Enabled = true;
        }

        private void btGuardarTorneio_Click(object sender, EventArgs e) {
            if (rbNormal.Checked) {
                StandardToutnament tourn;
                if (!flagEditarTorneio) {
                    tourn = new StandardToutnament();
                } else {
                    tourn = listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex);
                }
                tourn.Name = tbNomeTorneio.Text;
                tourn.Date = dpDataTorneio.Value;
                tourn.Descrition = tbDescricaoTorneio.Text;
                if (!flagEditarTorneio) {
                    if (tourRepo.AddTournament(tourn)) {
                        AtivarFormTorneio(false);
                        LimpaFormTorneio();
                        RefreshListTorneiosNormais();
                        lbTorneiosNormais.SelectedIndex = lbTorneiosNormais.Items.Count - 1;
                    }
                } else {
                    if (tourRepo.EditTournament(tourn)) {
                        AtivarFormTorneio(false);
                        LimpaFormTorneio();
                        RefreshListTorneiosEquipas();
                        flagEditarTorneio = false;
                    }
                }
            } else {
                TeamTournament tourn;
                if (!flagEditarTorneio) {
                    tourn = new TeamTournament();
                } else {
                    tourn = listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex);
                }
                tourn.Name = tbNomeTorneio.Text;
                tourn.Date = dpDataTorneio.Value;
                tourn.Descrition = tbDescricaoTorneio.Text;
                if (!flagEditarTorneio) {
                    if (tourRepo.AddTournament(tourn)) {
                        AtivarFormTorneio(false);
                        LimpaFormTorneio();
                        RefreshListTorneiosEquipas();
                        lbTorneiosEquipas.SelectedIndex = lbTorneiosEquipas.Items.Count - 1;
                    }
                } else {
                    if (tourRepo.EditTournament(tourn)) {
                        AtivarFormTorneio(false);
                        LimpaFormTorneio();
                        RefreshListTorneiosEquipas();
                        flagEditarTorneio = false;
                    }
                }
            }
            btNovoTorn.Enabled = true;
        }

        private void lbTorneiosNormais_SelectedIndexChanged(object sender, EventArgs e) {
            if (lbTorneiosNormais.SelectedIndex >= 0) {
                lbTorneiosEquipas.SelectedIndex = -1;
                flagEditarTorneio = true;
                PreencherTorneio(listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex));
                RefreshListJogos(listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex));
                LimpaFormJogos();
                AtivarFormJogo(false);
                rbNormal.Checked = true;
                rbEquipas.Enabled = false;
                rbNormal.Enabled = true;
            }
        }

        private void lbTorneiosEquipas_SelectedIndexChanged(object sender, EventArgs e) {
            if (lbTorneiosEquipas.SelectedIndex >= 0) {
                lbTorneiosNormais.SelectedIndex = -1;
                flagEditarTorneio = true;
                PreencherTorneio(listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex));
                RefreshListJogos(listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex));
                LimpaFormJogos();
                AtivarFormJogo(false);
                rbEquipas.Checked = true;
                rbEquipas.Enabled = true;
                rbNormal.Enabled = false;
            }
        }

        /// <summary>
        /// Altera o texto das lables e preenche a comboBox 
        /// mediante a escolha do radioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbEquipas_CheckedChanged(object sender, EventArgs e) {
            if (rbEquipas.Checked == true) {
                laJogEqu1.Text = "Equipa 1";
                laJogEqu2.Text = "Equipa 2";
            } else {
                laJogEqu1.Text = "Jogador 1";
                laJogEqu2.Text = "Jogador 2";
            }
        }

        /************************* FUNÇOES AUXILIARES **************************************/

        private void AtivarFormTorneio(bool flag) {
            if (!flagLoginArbitro) {
                gbTorneio.Enabled = flag;
                btGuardarTorn.Enabled = flag;
                btRemoverTorn.Enabled = flag;
                rbEquipas.Enabled = flag;
                rbNormal.Enabled = flag;
                if (flagEditarTorneio) {
                    btNovoJog.Enabled = true;
                    btRemoverTorn.Text = "Remover";
                } else {
                    btRemoverTorn.Text = "Cancelar";
                }
            }
        }

        private void LimpaFormTorneio() {
            tbDescricaoTorneio.Text = "";
            tbNomeTorneio.Text = "";
            dpDataTorneio.Value = DateTime.Now;
            lbTorneiosNormais.SelectedIndex = -1;
            lbTorneiosEquipas.SelectedIndex = -1;
            lbJogos.Items.Clear();
        }

        private void RefreshListTorneiosNormais() {
            LimpaFormTorneio();
            if (flagLoginArbitro) {
                listaStandardTourn = tourRepo.getStandardTournamentOfRefereeList(arbitro);
            } else {
                listaStandardTourn = tourRepo.getStandardTournamentList();
            }
            lbTorneiosNormais.Items.Clear();
            foreach (StandardToutnament torn in listaStandardTourn) {
                lbTorneiosNormais.Items.Add(torn.Name + "\t" + torn.Date.ToShortDateString());
            }
        }

        private void RefreshListTorneiosEquipas() {
            LimpaFormTorneio();
            if (flagLoginArbitro) {
                listaTeamTourn = tourRepo.getTeamTournamentOfRefereeList(arbitro);
            } else {
                listaTeamTourn = tourRepo.getTeamTournamentList();
            }
            lbTorneiosEquipas.Items.Clear();
            foreach (TeamTournament torn in listaTeamTourn) {
                lbTorneiosEquipas.Items.Add(torn.Name + "\t" + torn.Date.ToShortDateString());
            }
        }

        private void PreencherTorneio(Tournament tourn) {
            tbNomeTorneio.Text = tourn.Name;
            tbDescricaoTorneio.Text = tourn.Descrition;
            dpDataTorneio.Value = tourn.Date;
            AtivarFormTorneio(true);
        }


        /************************************************************************************/
        /*                                  JOGOS                                           */
        /************************************************************************************/
        
        /********************************* EVENTOS ******************************************/

        private void btNovoJog_Click(object sender, EventArgs e) {
            flagEditarJogo = false;
            AtivarFormJogo(true);
            btNovoJog.Enabled = false;
            LimpaFormJogos();
            fillReferee();
            fillBaralhos();
            if (rbNormal.Checked) {
                fillJogadores();
            } else {
                fillEquipas();
            }
        }

        private void btRemoverJog_Click(object sender, EventArgs e) {
            if (flagEditarJogo) {//MODO EDIÇAO
                if (rbNormal.Checked) {
                    gameRepo.DeleteGame(listaStandGames.ElementAt(lbJogos.SelectedIndex));
                    RefreshListJogos(listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex));
                } else {
                    gameRepo.DeleteGame(listaTeamGames.ElementAt(lbJogos.SelectedIndex));
                    RefreshListJogos(listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex));
                }
            }
            LimpaFormJogos();
            AtivarFormJogo(false);
            btNovoJog.Enabled = true;
        }

        private void btGuadarJog_Click(object sender, EventArgs e) {
            if (rbNormal.Checked) {
                StandardGame jogo;
                if (!flagEditarJogo) {
                    jogo = new StandardGame();
                } else {
                    jogo = listaStandGames.ElementAt(lbJogos.SelectedIndex);
                }
                jogo.Number = int.Parse("" + nudNumeroJog.Value);
                jogo.Date = dpDataJog.Value;
                jogo.Description = tbDescricaoJog.Text;
                jogo.Referee = listaArbitros.ElementAt(cbArbitro.SelectedIndex);
                jogo.DeckOne = listaDecks1.ElementAt(cbBaralho1.SelectedIndex);
                jogo.DeckTwo = listaDecks2.ElementAt(cbBaralho2.SelectedIndex);
                jogo.PlayerOne = listaPlayers1.ElementAt(cbJogEqu1.SelectedIndex);
                jogo.PlayerTwo = listaPlayers2.ElementAt(cbJogEqu2.SelectedIndex);
                jogo.Toutnament = listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex);
                if (!flagEditarJogo) {
                    if (gameRepo.AddGame(jogo)) {
                        AtivarFormJogo(false);
                        LimpaFormJogos();
                        RefreshListJogos(listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex));
                    }
                } else {
                    if (gameRepo.EditGame(jogo)) {
                        AtivarFormJogo(false);
                        LimpaFormJogos();
                        RefreshListJogos(listaStandardTourn.ElementAt(lbTorneiosNormais.SelectedIndex));
                        flagEditarJogo = false;
                    }
                }
            } else {
                TeamGame jogo;
                if (!flagEditarJogo) {
                    jogo = new TeamGame();
                } else {
                    jogo = listaTeamGames.ElementAt(lbJogos.SelectedIndex);
                }
                jogo.Number = int.Parse("" + nudNumeroJog.Value);
                jogo.Date = dpDataJog.Value;
                jogo.Description = tbDescricaoJog.Text;
                jogo.Referee = listaArbitros.ElementAt(cbArbitro.SelectedIndex);
                jogo.DeckOne = listaDecks1.ElementAt(cbBaralho1.SelectedIndex);
                jogo.DeckTwo = listaDecks2.ElementAt(cbBaralho2.SelectedIndex);
                jogo.TeamOne = listaTeams1.ElementAt(cbJogEqu1.SelectedIndex);
                jogo.TeamTwo = listaTeams2.ElementAt(cbJogEqu2.SelectedIndex);
                jogo.Tournament = listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex);
                if (!flagEditarJogo) {
                    if (gameRepo.AddGame(jogo)) {
                        AtivarFormJogo(false);
                        LimpaFormJogos();
                        RefreshListJogos(listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex));
                    }
                } else {
                    if (gameRepo.EditGame(jogo)) {
                        AtivarFormJogo(false);
                        LimpaFormJogos();
                        RefreshListJogos(listaTeamTourn.ElementAt(lbTorneiosEquipas.SelectedIndex));
                        flagEditarTorneio = false;
                    }
                }
            }
            btNovoJog.Enabled = true;
        }

        private void lbJogos_SelectedIndexChanged(object sender, EventArgs e) {
            if (lbJogos.SelectedIndex >= 0) {
                flagEditarJogo = true;
                PreencherJogo(lbJogos.SelectedIndex);              
                rbNormal.Enabled = false;
            }
        }

        /// <summary>
        /// Preenche a comboBox cbBaralho2 com os baralhos
        /// existentes, remove o que foi selecionado na 
        /// comboBox cbBaralho1 e seleciona o primeiro item
        /// </summary>
        private void cbBaralho1_SelectedIndexChanged(object sender, EventArgs e) {
            listaDecks2 = new List<Deck>(listaDecks1);
            listaDecks2.RemoveAt(cbBaralho1.SelectedIndex);
            cbBaralho2.Items.Clear();
            foreach (Deck deck in listaDecks2) {
                cbBaralho2.Items.Add(deck.Name);
            }
            if (listaDecks2.Count > 0) {
                cbBaralho2.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Preenche a comboBox cbJogEqu2 mediante o radioButton
        /// selecionado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbJogEqu1_SelectedIndexChanged(object sender, EventArgs e) {
            if (rbEquipas.Checked == true) {
                listaTeams2 = new List<Team>(listaTeams1);
                listaTeams2.RemoveAt(cbJogEqu1.SelectedIndex);
                //Eliminar as equipas com o mesmo player
                List<Team> tempList = new List<Team>();
                foreach(Player player in listaTeams1.ElementAt(cbJogEqu1.SelectedIndex).Player) {
                    foreach (Team equipe in listaTeams2) {
                        foreach (Player pla in equipe.Player) {
                            if (pla.Id == player.Id) {
                                tempList.Add(equipe);
                            }
                        }
                    }
                }
                foreach(Team equipa in tempList) {
                    listaTeams2.Remove(equipa);
                }

                cbJogEqu2.Items.Clear();
                foreach (Team team in listaTeams2) {
                    cbJogEqu2.Items.Add(team.Name);
                }
                if (listaTeams2.Count > 0) {
                    cbJogEqu2.SelectedIndex = 0;
                }else {
                    DadosIncuficientes("Equipas", "Equipas insuficientes para criar um jogo!\n"+
                        "Exitem jogadores que estam na mesma equipa!");
                }
            } else {
                listaPlayers2 = new List<Player>(listaPlayers1);
                listaPlayers2.RemoveAt(cbJogEqu1.SelectedIndex);
                cbJogEqu2.Items.Clear();
                foreach (Player player in listaPlayers2) {
                    cbJogEqu2.Items.Add(player.Name);
                }
                if (listaPlayers2.Count > 0) {
                    cbJogEqu2.SelectedIndex = 0;
                }
            }
        }

        /************************* FUNÇOES AUXILIARES **************************************/

        private void AtivarFormJogo(bool flag) {
            nudNumeroJog.Enabled = flag;
            dpDataJog.Enabled = flag;
            tbDescricaoJog.Enabled = flag;
            cbArbitro.Enabled = flag;
            cbBaralho2.Enabled = flag;
            cbJogEqu1.Enabled = flag;
            cbJogEqu2.Enabled = flag;
            cbBaralho1.Enabled = flag;
            btRemoverJog.Enabled = flag;
            btGuadarJog.Enabled = flag;
            if (flagEditarJogo) {
                btRemoverJog.Text = "Remover";
            } else {
                btRemoverJog.Text = "Cancelar";
            }
        }

        private void LimpaFormJogos() {
            nudNumeroJog.Value = 0;
            dpDataJog.Value = DateTime.Now;
            tbDescricaoJog.Text = "";
            cbArbitro.Items.Clear();
            cbBaralho1.Items.Clear();
            cbBaralho2.Items.Clear();
            cbJogEqu1.Items.Clear();
            cbJogEqu2.Items.Clear();
        }

        public void RefreshListJogos(StandardToutnament tourn) {
            lbJogos.Items.Clear();
            if (flagLoginArbitro) {
                listaStandGames = gameRepo.getStandardGamesOfRefereeList(tourn,arbitro);
            }else {
                listaStandGames = gameRepo.getStandardGamesList(tourn);
            }
            foreach (StandardGame game in listaStandGames) {
                lbJogos.Items.Add(game.Number + "\t" + game.Date.ToShortDateString());
            }
        }

        private void RefreshListJogos(TeamTournament tourn) {
            lbJogos.Items.Clear();
            if (flagLoginArbitro) {
                listaTeamGames = gameRepo.getTeamGamesOfRefereeList(tourn,arbitro);
            } else {
                listaTeamGames = gameRepo.getTeamGamesList(tourn);
            }
            listaTeamGames = gameRepo.getTeamGamesList(tourn);
            foreach (TeamGame game in listaTeamGames) {
                lbJogos.Items.Add(game.Number + "\t" + game.Date.ToShortDateString());
            }
        }

        private void PreencherJogo(int index) {
            AtivarFormJogo(true);
            fillBaralhos();
            fillReferee();
            Game jogo;
            if (rbNormal.Checked) {
                jogo = listaStandGames.ElementAt(index);
                fillJogadores();
                StandardGame Standjogo;
                Standjogo = listaStandGames.ElementAt(index);
                if (listaPlayers1.Count > 1 && listaDecks1.Count > 1) {
                    cbJogEqu1.SelectedItem = Standjogo.PlayerOne.Name;
                    cbJogEqu2.SelectedItem = Standjogo.PlayerTwo.Name;
                }

            } else {
                jogo = listaTeamGames.ElementAt(index);
                fillEquipas();
                TeamGame Teamjogo;
                Teamjogo = listaTeamGames.ElementAt(index);
                if (listaTeams1.Count > 1 && listaDecks1.Count > 1) {
                    cbJogEqu1.SelectedItem = Teamjogo.TeamOne.Name;
                    cbJogEqu2.SelectedItem = Teamjogo.TeamTwo.Name;
                }
            }
            nudNumeroJog.Value = jogo.Number;
            dpDataJog.Value = jogo.Date;
            tbDescricaoJog.Text = jogo.Description;
            cbArbitro.SelectedItem = jogo.Referee.Name;
            cbBaralho1.SelectedItem = jogo.DeckOne.Name;
            cbBaralho2.SelectedItem = jogo.DeckTwo.Name;
        }

        /// <summary>
        /// Preenche a comboBox cbBaralho1 com os arbitros
        /// existentes e seleciona o primeiro item
        /// </summary>
        public void fillReferee() {
            listaArbitros = arbitroRepo.GetRefereeList();
            cbArbitro.Items.Clear();
            foreach (Referee refer in listaArbitros) {
                cbArbitro.Items.Add(refer.Name);
            }
            if (listaArbitros.Count > 0) {
                cbArbitro.SelectedIndex = 0;
            }else {
                DadosIncuficientes("Arbitros", "Arbitros insuficientes para criar um jogo!");
            }
        }
        
        /// <summary>
        /// Preenche a comboBox cbBaralho1 com os baralhos
        /// existentes e seleciona o primeiro item
        /// </summary>
        public void fillBaralhos() {
            listaDecks1 = deckRepo.GetDecksList();
            cbBaralho1.Items.Clear();
            foreach (Deck deck in listaDecks1) {
                cbBaralho1.Items.Add(deck.Name);
            }
            if(listaDecks1.Count >= 2){
                cbBaralho1.SelectedIndex = 0;
            } else {
                DadosIncuficientes("Baralhos", "Baralhos insuficientes para criar um jogo!");
            }
        }

        /// <summary>
        /// Preenche a comboBox cbJogEqu1 com Jogadores e se existir alguma coisa
        /// na lista seleciona o primeiro item se não limpa a comboBox
        /// </summary>
        private void fillJogadores() {
            listaPlayers1 = playerRepo.GetPlayersList();
            cbJogEqu1.Items.Clear();
            foreach (Player player in listaPlayers1) {
                cbJogEqu1.Items.Add(player.Name);
            }
            if (listaPlayers1.Count >= 2) {
                cbJogEqu1.SelectedIndex = 0;
            } else {
                DadosIncuficientes("Jogadores", "Jogadores insuficientes para criar um jogo!");
            }
        }

        /// <summary>
        /// Preenche a comboBox cbJogEqu1 com Equipas e se existir alguma coisa
        /// na lista seleciona o primeiro item se não limpa a comboBox
        /// </summary>
        private void fillEquipas() {
            listaTeams1 = equipaRepo.GetTeamsList();
            cbJogEqu1.Items.Clear();
            foreach (Team team in listaTeams1) {
                cbJogEqu1.Items.Add(team.Name);
            }

            if (listaTeams1.Count >= 2) {
                cbJogEqu1.SelectedIndex = 0;
            } else {
                DadosIncuficientes("Equipa", "Equipas insuficientes para criar um jogo!");
            }
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e) {
            flagLoggingOut = true;
            formlogin.ativaCampos();
            formlogin.Show();
            this.Close();
        }
		/// <summary>
        /// Mostra mensagem de erro de dados incuficientes
        /// </summary>
        /// <param name="text">Texto da mensagem</param>
        /// <param name="title">Titulo da mensagem</param>
        private void DadosIncuficientes(string title,string text) {
            MessageBox.Show(this, text,
                title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            AtivarFormJogo(false);
            LimpaFormJogos();
        }
    }
}
