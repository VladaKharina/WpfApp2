using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AngryBirdsWPF
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private Point _projectilePosition, _dragStartPoint;
        private Vector _projectileVelocity;
        private bool _isDragging, _isFlying;
        private readonly Point _slingshotPosition = new Point(100, 300);
        private const double SlingshotLength = 50.0, Gravity = 0.8, GroundFriction = 0.2, SpeedFactor = 0.3;

        // Конструктор класса MainWindow
        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        // Метод инициализации игры
        private void InitializeGame()
        {
            _projectilePosition = _slingshotPosition; // Начальная позиция снаряда
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // Таймер для обновления экрана
            _timer.Tick += (sender, e) => OnTimerTick(); // Обработчик событий для таймера
            _timer.Start(); // Запуск таймера
            UpdateProjectilePosition(); // Обновление позиции снаряда
        }

        // Обработчик нажатия мыши
        private void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = IsInsideProjectile(e.GetPosition(GameCanvas)); // Проверка, попадает ли курсор на снаряд
                _dragStartPoint = e.GetPosition(GameCanvas); // Начальная точка перетаскивания
                if (!_isDragging) ResetProjectile(); // Сброс снаряда, если не перетаскивается
            }
        }

        // Обработчик перемещения мыши
        private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _projectilePosition = e.GetPosition(GameCanvas); // Обновление позиции снаряда при перетаскивании
                UpdateProjectilePosition(); // Обновление отображения позиции снаряда
                UpdateSlingshot(); // Обновление отображения рогатки
            }
        }

        // Обработчик отпускания кнопки мыши
        private void GameCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _isFlying = true; // Начало полета снаряда
                _projectileVelocity = (_dragStartPoint - e.GetPosition(GameCanvas)) * SpeedFactor; // Вычисление скорости снаряда
                HideSlingshot(); // Скрытие рогатки
            }
        }

        // Метод обработки тиков таймера
        private void OnTimerTick()
        {
            if (_isFlying)
            {
                _projectilePosition += _projectileVelocity; // Обновление позиции снаряда по его скорости
                _projectileVelocity.Y += Gravity; // Применение гравитации

                // Проверка столкновения с землей
                if (_projectilePosition.Y > GameCanvas.ActualHeight - 30)
                {
                    _projectilePosition.Y = GameCanvas.ActualHeight - 30;
                    _projectileVelocity.Y = -_projectileVelocity.Y * GroundFriction; // Отскок снаряда
                    if (Math.Abs(_projectileVelocity.Y) < 1)
                        ResetProjectile();
                    // Перезапуск, если скорость мала
                }

                CheckCollisionWithPigs(); // Проверка столкновения с "свиньями"
                UpdateProjectilePosition(); // Обновление отображения позиции снаряда
            }
        }

        // Метод обновления позиции снаряда на канвасе
        private void UpdateProjectilePosition()
        {
            Canvas.SetLeft(Projectile, _projectilePosition.X - Projectile.Width / 2);
            Canvas.SetTop(Projectile, _projectilePosition.Y - Projectile.Height / 2);
        }
        // Метод обновления отображения рогатки
        private void UpdateSlingshot()
        {
            if (_isDragging)
            {
                SetSlingshotBands(_projectilePosition); // Установка полос рогатки
            }
            else
            {
                HideSlingshot(); // Скрытие рогатки
            }
        }

        // Метод установки полос рогатки
        private void SetSlingshotBands(Point projectileCenter)
        {
            LeftBand.X1 = _slingshotPosition.X - SlingshotLength;
            LeftBand.Y1 = _slingshotPosition.Y;
            LeftBand.X2 = projectileCenter.X;
            LeftBand.Y2 = projectileCenter.Y;
            RightBand.X1 = _slingshotPosition.X + SlingshotLength;
            RightBand.Y1 = _slingshotPosition.Y;
            RightBand.X2 = projectileCenter.X;
            RightBand.Y2 = projectileCenter.Y;
        }

        // Метод скрытия рогатки
        private void HideSlingshot()
        {
            SetSlingshotBands(_slingshotPosition); // Возвращение полос рогатки в исходное положение
        }

        // Метод проверки столкновения с "свиньями"
        private void CheckCollisionWithPigs()
        {
            foreach (UIElement element in GameCanvas.Children)
            {
                if (element is Ellipse pig && element != Projectile)
                {
                    var pigRect = new Rect(Canvas.GetLeft(pig), Canvas.GetTop(pig), pig.Width, pig.Height);
                    var projectileRect = new Rect(Canvas.GetLeft(Projectile), Canvas.GetTop(Projectile), Projectile.Width, Projectile.Height);

                    if (pigRect.IntersectsWith(projectileRect))
                    {
                        GameCanvas.Children.Remove(pig); // Удаление "свиньи" при столкновении
                        _isFlying = false; // Остановка снаряда
                        break;
                    }
                }
            }

            // Проверка оставшихся свинок 
            bool pigsRemaining = false;
            foreach (UIElement element in GameCanvas.Children)
            {
                if (element is Ellipse pig && element != Projectile)
                {
                    pigsRemaining = true;
                    break;
                }
            }

            // Если свинок не осталось, показать сообщение о победе
            if (!pigsRemaining)
            {
                MessageBox.Show("Вы победили!");
            }
        }

        // Метод проверки, находится ли точка внутри снаряда
        private bool IsInsideProjectile(Point point)
        {
            var distance = (point - _projectilePosition).Length;
            return distance <= Projectile.Width / 2;
        }

        // Метод сброса позиции снаряда
        private void ResetProjectile()
        {
            _projectilePosition = _slingshotPosition; // Возвращение снаряда в исходное положение
            _projectileVelocity = new Vector(0, 0); // Сброс скорости
            _isFlying = false; // Остановка полета
            UpdateProjectilePosition(); // Обновление отображения позиции снаряда
            HideSlingshot(); // Скрытие рогатки
        }
    }
}