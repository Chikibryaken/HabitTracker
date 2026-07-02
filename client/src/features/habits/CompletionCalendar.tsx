import type { Completion } from "../../types/habit";
import { formatLocalDate } from "./dateUtils";

interface CompletionCalendarProps {
  completions: Completion[];
  today: string;
}

const WEEKDAY_LABELS = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

export default function CompletionCalendar({ completions, today }: CompletionCalendarProps) {
  const completedDates = new Set(completions.map((completion) => completion.date));

  const [yearStr, monthStr] = today.split("-");
  const year = Number(yearStr);
  const month = Number(monthStr);

  const daysInMonth = new Date(year, month, 0).getDate();
  const firstDayOfMonth = new Date(year, month - 1, 1);
  const leadingBlankCount = (firstDayOfMonth.getDay() + 6) % 7;

  const days = Array.from({ length: daysInMonth }, (_, index) => index + 1);

  return (
    <div className="calendar-wrapper">
      <div className="calendar-grid calendar-weekdays">
        {WEEKDAY_LABELS.map((label) => (
          <div key={label} className="calendar-weekday">
            {label}
          </div>
        ))}
      </div>
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
            <div key={cellDate} className={cellClassNames.join(" ")} title={cellDate}>
              {day}
            </div>
          );
        })}
      </div>
    </div>
  );
}
