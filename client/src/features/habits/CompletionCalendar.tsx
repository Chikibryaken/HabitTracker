import { formatLocalDate } from "./dateUtils";
import { useHabitCompletions } from "./useHabitCompletions";
import { useToggleCompletion } from "./useToggleCompletion";

interface CompletionCalendarProps {
  habitId: string;
  today: string;
  year: number;
  month: number;
  onNavigate: (year: number, month: number) => void;
}

const WEEKDAY_LABELS = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
export const MONTH_FORMATTER = new Intl.DateTimeFormat("en-US", { month: "long", year: "numeric" });

function lastDayOfMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate();
}

export default function CompletionCalendar({ habitId, today, year, month, onNavigate }: CompletionCalendarProps) {
  const [todayYearStr, todayMonthStr] = today.split("-");
  const todayYear = Number(todayYearStr);
  const todayMonth = Number(todayMonthStr);

  const monthStart = formatLocalDate(new Date(year, month - 1, 1));
  const monthEnd = formatLocalDate(new Date(year, month - 1, lastDayOfMonth(year, month)));

  const { data: completions, isLoading } = useHabitCompletions(habitId, monthStart, monthEnd);
  const toggleCompletion = useToggleCompletion();

  const completedDates = new Set((completions ?? []).map((completion) => completion.date));

  const daysInMonth = lastDayOfMonth(year, month);
  const firstDayOfMonth = new Date(year, month - 1, 1);
  const leadingBlankCount = (firstDayOfMonth.getDay() + 6) % 7;
  const days = Array.from({ length: daysInMonth }, (_, index) => index + 1);

  const isCurrentMonth = year === todayYear && month === todayMonth;

  const goToPreviousMonth = (): void => {
    if (month === 1) {
      onNavigate(year - 1, 12);
    } else {
      onNavigate(year, month - 1);
    }
  };

  const goToNextMonth = (): void => {
    if (isCurrentMonth) {
      return;
    }
    if (month === 12) {
      onNavigate(year + 1, 1);
    } else {
      onNavigate(year, month + 1);
    }
  };

  const handleDayClick = (date: string, isCompleted: boolean): void => {
    toggleCompletion.mutate({ habitId, date, isCurrentlyCompleted: isCompleted });
  };

  return (
    <div className="calendar-wrapper">
      <div className="calendar-nav">
        <button type="button" onClick={goToPreviousMonth} aria-label="Previous month">
          &lsaquo;
        </button>
        <span className="calendar-title">{MONTH_FORMATTER.format(new Date(year, month - 1, 1))}</span>
        <button
          type="button"
          onClick={goToNextMonth}
          disabled={isCurrentMonth}
          aria-label="Next month"
        >
          &rsaquo;
        </button>
      </div>

      <div className="calendar-grid calendar-weekdays">
        {WEEKDAY_LABELS.map((label) => (
          <div key={label} className="calendar-weekday">
            {label}
          </div>
        ))}
      </div>

      {isLoading ? (
        <p className="page-status">Loading calendar...</p>
      ) : (
        <div className="calendar-grid">
          {Array.from({ length: leadingBlankCount }, (_, index) => (
            <div key={`blank-${index}`} className="calendar-cell calendar-cell-blank" />
          ))}
          {days.map((day) => {
            const cellDate = formatLocalDate(new Date(year, month - 1, day));
            const isCompleted = completedDates.has(cellDate);
            const isFuture = cellDate > today;

            const cellClassNames = ["calendar-cell"];
            if (isFuture) {
              cellClassNames.push("calendar-cell-future");
            } else if (isCompleted) {
              cellClassNames.push("calendar-cell-completed");
            } else {
              cellClassNames.push("calendar-cell-missed");
            }

            return (
              <button
                key={cellDate}
                type="button"
                className={cellClassNames.join(" ")}
                onClick={() => handleDayClick(cellDate, isCompleted)}
                disabled={isFuture}
                title={cellDate}
              >
                {day}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
