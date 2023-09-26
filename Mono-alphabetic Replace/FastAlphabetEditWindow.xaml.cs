using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Mono_alphabetic_Replace
{
    /// <summary>
    /// Логика взаимодействия для FastAlphabetEditWindow.xaml
    /// </summary>
    public partial class FastAlphabetEditWindow
    {
        private readonly MainWindow _mainWindow;

        public FastAlphabetEditWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            Owner = mainWindow;
            _mainWindow = mainWindow;
        }

        private void TopPanel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {
                /* ignored */
            }
        }

        private void ExitButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => CloseWindow();

        private void CloseWindow()
        {
            Close();
            _mainWindow.Focus();
        }

        private void ShiftNumberButton_OnClick(object sender, RoutedEventArgs e)
        {
            void NumberShiftToAlphabetSize(ref int n, int alphabetLength)
            {
                if (n < 0) n = Math.Abs(n);
                while (n > alphabetLength) n -= alphabetLength;
            }

            var tryParse = int.TryParse(ShiftNumberTextBox.Text, out var number);
            if (tryParse is false)
            {
                MessageBox.Show(this, "Введите корректное число для смещения!", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var newAlphabet = _mainWindow.EditedAlphabet;

            if (number > 0)
            {
                NumberShiftToAlphabetSize(ref number, newAlphabet.Length);
                newAlphabet = newAlphabet.TakeLast(number).Concat(newAlphabet.SkipLast(number)).ToArray();
            }
            else if (number < 0)
            {
                NumberShiftToAlphabetSize(ref number, newAlphabet.Length);
                newAlphabet = newAlphabet.Skip(number).Concat(newAlphabet.Take(number)).ToArray();
            }

            _mainWindow.EditedAlphabet = newAlphabet;
            CloseWindow();
        }

        private void ShiftWordButton_OnClick(object sender, RoutedEventArgs e)
        {
            var text = ShiftWordTextBox.Text;
            if (text.GroupBy(p => p).Any(p => p.Count() > 1))
            {
                MessageBox.Show(this, "В слове не должно быть повторяющихся", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var editedAlphabet = _mainWindow.EditedAlphabet.Select(char.ToLower).ToArray();
            if (text.Length > editedAlphabet.Length)
            {
                MessageBox.Show(this, "Слово должно быть короче алфавита", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (text.Any(p => editedAlphabet.Contains(char.ToLower(p)) is false))
            {
                MessageBox.Show(this, "Слово должно состоять только с букв алфавита", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _mainWindow.EditedAlphabet = text.ToCharArray().Concat(editedAlphabet.Except(text.ToLower())).ToArray();
            CloseWindow();
        }

        private void FastAlphabetEditWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var spawnAtRight = _mainWindow.Left + _mainWindow.Width + 10;
            var spawnAtLeft = _mainWindow.Left - Width - 10;
            if (spawnAtRight + Width < SystemParameters.WorkArea.Right) Left = spawnAtRight;
            else if (spawnAtLeft > 0) Left = spawnAtLeft;
        }
    }
}