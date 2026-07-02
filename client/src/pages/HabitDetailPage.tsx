import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import CompletionCalendar, { MONTH_FORMATTER } from "../features/habits/CompletionCalendar";
import { formatLocalDate } from "../features/habits/dateUtils";
import { useHabit } from "../features/habits/useHabits";
import { useHabitStats } from "../features/habits/useHabitStats";

export default function HabitDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const habitId = id ?? "";

  const today = formatLocalDate(new Date());
  const [todayYearStr, todayMonthStr] = today.split("-");
  const todayYear = Number(todayYearStr);
  const todayMonth = Number(todayMonthStr);

  const [viewedYear, setViewedYear] = useState(todayYear);
  const [viewedMonth, setViewedMonth] = useState(todayMonth);

  const isViewingCurrentMonth = viewedYear === todayYear && viewedMonth === todayMonth;
  // Reference date for month stats: real "today" for the current month, otherwise the last
  // day of the viewed month — the stats endpoint treats that date's day-of-month as "days
  // elapsed", so a past month's last day naturally yields the full month's totals.
  const monthStatsDate = isViewingCurrentMonth
    ? today
    : formatLocalDate(new Date(viewedYear, viewedMonth, 0));

  const { data: habit, isLoading: isHabitLoading, isError: isHabitError } = useHabit(habitId);
  // Streak is a "right now" property of the habit and stays anchored to the real today,
  // independent of whichever month is being browsed in the calendar below.
  const { data: streakStats, isLoading: isStreakLoading } = useHabitStats(habitId, today);
  const { data: monthStats, isLoading: isMonthStatsLoading } = useHabitStats(habitId, monthStatsDate);

  if (isHabitLoading) {
    return <p className="page-status">Loading habit...</p>;
  }

  if (isHabitError || !habit) {
    return <p className="page-status page-error">Failed to load habit.</p>;
  }

  const streakLabel = habit.frequency === "Daily" ? "day streak" : "week streak";
  const monthLabel = isViewingCurrentMonth
    ? "this month"
    : MONTH_FORMATTER.format(new Date(viewedYear, viewedMonth - 1, 1));

  return (
    <div className="habit-detail-page">
      <button type="button" className="back-button" onClick={() => navigate("/dashboard")}>
        &larr; Back to dashboard
      </button>

      <header className="habit-detail-header">
        <h1>{habit.name}</h1>
        <span className={`habit-badge habit-badge-${habit.frequency.toLowerCase()}`}>
          {habit.frequency}
        </span>
      </header>

      {habit.description && <p className="habit-detail-description">{habit.description}</p>}

      {isStreakLoading || isMonthStatsLoading || !streakStats || !monthStats ? (
        <p className="page-status">Loading stats...</p>
      ) : (
        <div className="stats-panel">
          <div className="stat-block">
            <span className="stat-value">{streakStats.currentStreak}</span>
            <span className="stat-label">{streakLabel}</span>
          </div>
          <div className="stat-block">
            <span className="stat-value">{monthStats.monthlyCompletionRate}%</span>
            <span className="stat-label">{monthLabel}</span>
          </div>
          <div className="stat-block">
            <span className="stat-value">
              {monthStats.completedThisMonth} / {monthStats.daysElapsedThisMonth}
            </span>
            <span className="stat-label">days completed</span>
          </div>
        </div>
      )}

      <CompletionCalendar
        habitId={habitId}
        today={today}
        year={viewedYear}
        month={viewedMonth}
        onNavigate={(year, month) => {
          setViewedYear(year);
          setViewedMonth(month);
        }}
      />
    </div>
  );
}
