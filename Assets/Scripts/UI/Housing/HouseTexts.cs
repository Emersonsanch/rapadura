using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Housing;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// Localization keys (en + pt-BR) of "Minha casa" (<see cref="MyHouseButton"/>, <see cref="HouseScreenController"/>).
    /// Same self-contained pattern as <c>VillageChestTexts</c>: merged into the live manager on first use (keys it
    /// already has win), pt-BR fallback without a manager. Item names live in <see cref="HouseItemTexts"/>.
    /// </summary>
    public static class HouseTexts
    {
        public const string Button = "ui.house.button";
        public const string ButtonNews = "ui.house.button_news";
        /// <summary>{0} = level name.</summary>
        public const string Title = "ui.house.title";
        /// <summary>{0}/{1} interior items, {2}/{3} yard items, {4} depot entries.</summary>
        public const string Counts = "ui.house.counts";
        public const string Level1 = "ui.house.level_1";
        public const string Level2 = "ui.house.level_2";
        public const string Level3 = "ui.house.level_3";
        public const string Level4 = "ui.house.level_4";
        public const string Level5 = "ui.house.level_5";
        public const string Arrange = "ui.house.arrange";
        public const string ArrangeDone = "ui.house.arrange_done";
        public const string ChestButton = "ui.house.chest_button";
        public const string ChestTitle = "ui.house.chest_title";
        public const string ChestSubtitle = "ui.house.chest_subtitle";
        public const string SleepButton = "ui.house.sleep_button";
        public const string SleepQuestion = "ui.house.sleep_question";
        public const string SleepYes = "ui.house.sleep_yes";
        public const string SleepNo = "ui.house.sleep_no";
        public const string Slept = "ui.house.slept";
        public const string TurnLeft = "ui.house.turn_left";
        public const string TurnRight = "ui.house.turn_right";
        public const string ShowYard = "ui.house.show_yard";
        public const string ShowInside = "ui.house.show_inside";
        public const string Leave = "ui.house.leave";
        public const string Depot = "ui.house.depot";
        public const string DepotEmpty = "ui.house.depot_empty";
        /// <summary>{0} = item name, {1} = copies.</summary>
        public const string DepotEntry = "ui.house.depot_entry";
        public const string LayerFloor = "ui.house.layer_floor";
        public const string LayerRug = "ui.house.layer_rug";
        public const string LayerWall = "ui.house.layer_wall";
        public const string LayerCeiling = "ui.house.layer_ceiling";
        public const string LayerSurface = "ui.house.layer_surface";
        public const string LayerYard = "ui.house.layer_yard";
        public const string Confirm = "ui.house.confirm";
        public const string Rotate = "ui.house.rotate";
        public const string Mirror = "ui.house.mirror";
        public const string Store = "ui.house.store";
        public const string Cancel = "ui.house.cancel";
        public const string HintPc = "ui.house.hint_pc";
        public const string HintTouch = "ui.house.hint_touch";
        public const string HintView = "ui.house.hint_view";
        /// <summary>{0} = item name.</summary>
        public const string StatusPlaced = "ui.house.status_placed";
        /// <summary>{0} = item name.</summary>
        public const string StatusStored = "ui.house.status_stored";
        /// <summary>{0} = item name.</summary>
        public const string StatusCarrying = "ui.house.status_carrying";
        public const string StatusChestInUse = "ui.house.status_chest_in_use";
        public const string StatusWardrobe = "ui.house.status_wardrobe";
        public const string StatusGuestBook = "ui.house.status_guest_book";
        public const string StatusPetBed = "ui.house.status_pet_bed";
        public const string StatusNoBed = "ui.house.status_no_bed";
        public const string StatusNoChest = "ui.house.status_no_chest";
        /// <summary>{0} = item name.</summary>
        public const string StatusItem = "ui.house.status_item";
        public const string QuestTitle = "ui.house.quest_title";
        public const string QuestGrandma = "ui.house.quest_grandma";
        public const string QuestAccept = "ui.house.quest_accept";
        public const string QuestLater = "ui.house.quest_later";
        public const string QuestDone = "ui.house.quest_done";
        public const string ReactMaithe = "ui.house.react_maithe";
        public const string ReactMaria = "ui.house.react_maria";
        public const string ReactJoaquim = "ui.house.react_joaquim";
        public const string ReactIcaro = "ui.house.react_icaro";
        public const string ReactLavine = "ui.house.react_lavine";
        public const string ResultPrefix = "ui.house.result.";

        public static string ResultKey(HousePlacementResult result) => ResultPrefix + result.ToString().ToLowerInvariant();

        public static string LevelKey(int level) => HouseLevels.Get(level).NameKey;

        public static string LayerKey(HouseLayer layer)
        {
            switch (layer)
            {
                case HouseLayer.Rug: return LayerRug;
                case HouseLayer.Wall: return LayerWall;
                case HouseLayer.Ceiling: return LayerCeiling;
                case HouseLayer.Surface: return LayerSurface;
                case HouseLayer.Yard: return LayerYard;
                default: return LayerFloor;
            }
        }

        public static string ReactionKey(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maria: return ReactMaria;
                case CharacterId.Joaquim: return ReactJoaquim;
                case CharacterId.Icaro: return ReactIcaro;
                case CharacterId.Lavine: return ReactLavine;
                default: return ReactMaithe;
            }
        }

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Button, "My house", "Minha casa"),
            new LocalizationEntry(ButtonNews, "My house (!)", "Minha casa (!)"),
            new LocalizationEntry(Title, "My house · {0}", "Minha casa · {0}"),
            new LocalizationEntry(Counts, "Inside {0}/{1} · Yard {2}/{3} · Storage {4}", "Dentro {0}/{1} · Quintal {2}/{3} · Depósito {4}"),
            new LocalizationEntry(Level1, "Wooden house", "Casa de madeira"),
            new LocalizationEntry(Level2, "Wooden house with porch", "Casa de madeira com varanda"),
            new LocalizationEntry(Level3, "Rammed-earth house with tiles", "Casa de taipa e telha"),
            new LocalizationEntry(Level4, "Brick house", "Casa de alvenaria"),
            new LocalizationEntry(Level5, "Farmhouse manor", "Casarão"),
            new LocalizationEntry(Arrange, "Tidy up the house", "Arrumar a casa"),
            new LocalizationEntry(ArrangeDone, "Done tidying", "Pronto"),
            new LocalizationEntry(ChestButton, "Chest", "Baú"),
            new LocalizationEntry(ChestTitle, "House chest", "Baú da casa"),
            new LocalizationEntry(ChestSubtitle, "Game items go here; house items go to the Storage.", "Aqui vão as coisas do jogo; item de casa vai pro Depósito."),
            new LocalizationEntry(SleepButton, "Sleep", "Dormir"),
            new LocalizationEntry(SleepQuestion, "Sleep until morning? The game will be saved.", "Dormir até amanhã? O jogo vai ser salvo."),
            new LocalizationEntry(SleepYes, "Sleep", "Dormir"),
            new LocalizationEntry(SleepNo, "Not now", "Agora não"),
            new LocalizationEntry(Slept, "Good morning! You slept until 7 and the game was saved.", "Bom dia! Dormiu até as 7h e o jogo foi salvo."),
            new LocalizationEntry(TurnLeft, "⟲ Turn", "⟲ Girar"),
            new LocalizationEntry(TurnRight, "Turn ⟳", "Girar ⟳"),
            new LocalizationEntry(ShowYard, "Yard", "Quintal"),
            new LocalizationEntry(ShowInside, "Inside", "Dentro"),
            new LocalizationEntry(Leave, "Leave", "Sair"),
            new LocalizationEntry(Depot, "Storage", "Depósito"),
            new LocalizationEntry(DepotEmpty, "Nothing here for this spot. House items you get arrive in the Storage.", "Nada pra este lugar. Os itens de casa que você ganha chegam no Depósito."),
            new LocalizationEntry(DepotEntry, "{0} ×{1}", "{0} ×{1}"),
            new LocalizationEntry(LayerFloor, "Floor", "Piso"),
            new LocalizationEntry(LayerRug, "Rug", "Tapete"),
            new LocalizationEntry(LayerWall, "Wall", "Parede"),
            new LocalizationEntry(LayerCeiling, "Ceiling", "Teto"),
            new LocalizationEntry(LayerSurface, "On top", "Superfície"),
            new LocalizationEntry(LayerYard, "Yard", "Quintal"),
            new LocalizationEntry(Confirm, "✓ Place", "✓ Colocar"),
            new LocalizationEntry(Rotate, "↺ Rotate", "↺ Girar"),
            new LocalizationEntry(Mirror, "⇋ Mirror", "⇋ Espelhar"),
            new LocalizationEntry(Store, "Put away", "Guardar"),
            new LocalizationEntry(Cancel, "Cancel", "Cancelar"),
            new LocalizationEntry(HintPc, "Pick an item from the Storage or click one in the house · Q/E rotate · click places · Del puts away · right click cancels", "Escolha um item do Depósito ou clique num da casa · Q/E gira · clique coloca · Del guarda · botão direito cancela"),
            new LocalizationEntry(HintTouch, "Pick an item, drag with one finger, twist with two · ✓ places", "Escolha um item, arraste com um dedo, gire com dois · ✓ coloca"),
            new LocalizationEntry(HintView, "Tap the chest or the bed to use them.", "Toque no baú ou na cama para usar."),
            new LocalizationEntry(StatusPlaced, "{0} is in place.", "{0} no lugar."),
            new LocalizationEntry(StatusStored, "{0} went back to the Storage.", "{0} voltou pro Depósito."),
            new LocalizationEntry(StatusCarrying, "{0}: choose where it goes.", "{0}: escolha onde fica."),
            new LocalizationEntry(StatusChestInUse, "Take things out of the chest before putting this extra chest away.", "Tire coisas do baú antes de guardar este baú extra."),
            new LocalizationEntry(StatusWardrobe, "The wardrobe is still being sorted out — soon.", "O guarda-roupa ainda está sendo arrumado — em breve."),
            new LocalizationEntry(StatusGuestBook, "Guest book: nobody has signed it yet.", "Livro de visitas: ninguém assinou ainda."),
            new LocalizationEntry(StatusPetBed, "Caramelo sniffs it and wags his tail.", "O Caramelo cheira e abana o rabo."),
            new LocalizationEntry(StatusNoBed, "Put a bed in the house first (Tidy up the house).", "Coloque uma cama na casa primeiro (Arrumar a casa)."),
            new LocalizationEntry(StatusNoChest, "Put the chest in the house first (Tidy up the house).", "Coloque o baú na casa primeiro (Arrumar a casa)."),
            new LocalizationEntry(StatusItem, "{0}", "{0}"),
            new LocalizationEntry(QuestTitle, "A Corner of Your Own", "Um Cantinho Seu"),
            new LocalizationEntry(QuestGrandma,
                "Grandma Zefa: \"Your grandpa planted a cajá tree for every grandchild that was born. This one is yours. The land around it is yours too — and the little wooden house he put up to rest on his drover trips is your home now. It leaks, it has termites, but it has love.\"",
                "Vó Zefa: \"O seu avô plantava um pé de cajá pra cada neto que nascia. Este aqui é o seu. A terra em volta dele é sua também — e a casinha de madeira que ele levantou pra descansar nas viagens de tropeiro agora é sua casa. Tem goteira, tem cupim, mas tem amor.\""),
            new LocalizationEntry(QuestAccept, "See the house", "Ver a casa"),
            new LocalizationEntry(QuestLater, "Later", "Depois"),
            new LocalizationEntry(QuestDone, "Quest complete: A Corner of Your Own! Grandma's gifts are in the Storage.", "Missão concluída: Um Cantinho Seu! Os presentes da Vó estão no Depósito."),
            new LocalizationEntry(ReactMaithe, "Maithe: \"A window to watch the wind go by. Grandma, it's perfect.\"", "Maithe: \"Uma janela pra ver o vento passar. Vó, é perfeita.\""),
            new LocalizationEntry(ReactMaria, "Maria: \"I'll plant rosemary by the window. You'll see, Grandma.\"", "Maria: \"Vou plantar alecrim na janela. A senhora vai ver, Vó.\""),
            new LocalizationEntry(ReactJoaquim, "Joaquim: \"Leaks and termites? Leave it to me, I'll fix it.\"", "Joaquim: \"Goteira e cupim? Deixa comigo, eu conserto.\""),
            new LocalizationEntry(ReactIcaro, "Ícaro: \"Could I hook a contraption to the zinc roof? ...Kidding. Mostly.\"", "Ícaro: \"Dá pra ligar uma engenhoca no telhado de zinco? ...Brincadeira. Quase.\""),
            new LocalizationEntry(ReactLavine, "Lavine: \"A corner all mine... and Caramelo's, right?\"", "Lavine: \"Um cantinho só meu... e do Caramelo, né?\""),
            Result(HousePlacementResult.Ok, "Fits here.", "Cabe aqui."),
            Result(HousePlacementResult.UnknownItem, "That item doesn't exist anymore.", "Esse item não existe mais."),
            Result(HousePlacementResult.NotInDepot, "There's none left in the Storage.", "Não tem mais nenhum no Depósito."),
            Result(HousePlacementResult.UnknownInstance, "That piece isn't in the house anymore.", "Essa peça não está mais na casa."),
            Result(HousePlacementResult.WrongLayer, "This one doesn't go there.", "Esse não vai aí."),
            Result(HousePlacementResult.LevelTooLow, "Too big for this house — it needs a bigger one.", "Grande demais pra esta casa — precisa de uma maior."),
            Result(HousePlacementResult.OutOfBounds, "It doesn't fit inside the space.", "Não cabe dentro do espaço."),
            Result(HousePlacementResult.Overlaps, "Something's already there.", "Já tem coisa aí."),
            Result(HousePlacementResult.OverOpening, "Not over the door or the window.", "Em cima da porta ou da janela, não."),
            Result(HousePlacementResult.CurtainNeedsWindow, "Curtains only go over a window.", "Cortina só vai na janela."),
            Result(HousePlacementResult.BlocksDoor, "That would block the way in.", "Isso tranca a passagem da porta."),
            Result(HousePlacementResult.NeedsFreeFront, "It needs a free spot in front to be used.", "Precisa de um espaço livre na frente pra usar."),
            Result(HousePlacementResult.BlocksAccess, "That would leave something you can't reach.", "Assim alguma coisa fica sem como chegar perto."),
            Result(HousePlacementResult.NoSurfaceParent, "Put it on top of a table or a shelf.", "Coloque em cima de uma mesa ou prateleira."),
            Result(HousePlacementResult.SurfaceSlotTaken, "That spot on top is taken.", "Esse lugar em cima já está ocupado."),
            Result(HousePlacementResult.RotationNotAllowed, "This one doesn't turn.", "Esse não gira."),
            Result(HousePlacementResult.InteriorLimitReached, "The house is full — put something away first.", "A casa está cheia — guarde alguma coisa antes."),
            Result(HousePlacementResult.YardLimitReached, "The yard is full — put something away first.", "O quintal está cheio — guarde alguma coisa antes."),
            Result(HousePlacementResult.FixedDecoration, "That's your cajá tree's spot.", "Ali é o lugar do seu pé de cajá."),
            Result(HousePlacementResult.TooManyExtraChests, "This house only holds one extra chest.", "Esta casa só aguenta um baú extra."),
            Result(HousePlacementResult.DepotFull, "The Storage is full.", "O Depósito está cheio."),
        };

        private static LocalizationEntry Result(HousePlacementResult result, string en, string ptBR) => new LocalizationEntry(ResultKey(result), en, ptBR);

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
            HouseItemTexts.EnsureRegistered(manager);
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key)) return manager.Get(key);
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null) return null;
            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries) table[entry.Key] = entry.PtBR;
                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
