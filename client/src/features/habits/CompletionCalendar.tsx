import { useState } from "react";
import { formatLocalDate } from "./dateUtils";
import { useHabitCompletions } from "./useHabitCompletions";
import { useToggleCompletion } from "./useToggleCompletion";

interface CompletionCalendarProps {
  habitId: string;
  today: string;
}

const WEEKDAY_LABELS = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
const MONTH_FORMATTER = new Intl.DateTimeFormat("en-US", { month: "long", year: "numeric" });

function lastDayOfMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate();
}

export default function CompletionCalendar({ habitId, today }: CompletionCalendarProps) {
  const [todayYearStr, todayMonthStr] = today.split("-");
  const todayYear = Number(todayYearStr);
  const todayMonth = Number(todayMonthStr);

  const [viewedYear, setViewedYear] = useState(todayYear);
  const [viewedMonth, setViewedMonth] = useState(todayMonth);

  const monthStart = formatLocalDate(new Date(viewedYear, viewedMonth - 1, 1));
  const monthEnd = formatLocalDate(new Date(viewedYear, viewedMonth - 1, lastDayOfMonth(viewedYear, viewedMonth)));

  const { data: completions, isLoading } = useHabitCompletions(habitId, monthStart, monthEnd);
  const toggleCompletion = useToggleCompletion();

  const completedDates = new Set((completions ?? []).map((completion) => completion.date));

  const daysInMonth = lastDayOfMonth(viewedYear, viewedMonth);
  const firstDayOfMonth = new Date(viewedYear, viewedMonth - 1, 1);
  const leadingBlankCount = (firstDayOfMonth.getDay() + 6) % 7;
  const days = Array.from({ length: daysInMonth }, (_, index) => index + 1);

  const isCurrentMonth = viewedYear === todayYear && viewedMonth === todayMonth;

  const goToPreviousMonth = (): void => {
    if (viewedMonth === 1) {
      setViewedYear((year) => year - 1);
      setViewedMonth(12);
    } else {
      setViewedMonth((month) => month - 1);
    }
  };

  const goToNextMonth = (): void => {
    if (isCurrentMonth) {
      return;
    }
    if (viewedMonth === 12) {
      setViewedYear((year) => year + 1);
      setViewedMonth(1);
    } else {
      setViewedMonth((month) => month + 1);
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
        <span className="calendar-title">
          {MONTH_FORMATTER.format(new Date(viewedYear, viewedMonth - 1, 1))}
        </span>
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
            const cellDate = formatLocalDate(new Date(viewedYear, viewedMonth - 1, day));
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
