import { useNavigate, useParams } from "react-router-dom";
import CompletionCalendar from "../features/habits/CompletionCalendar";
import { formatLocalDate } from "../features/habits/dateUtils";
import { useHabit } from "../features/habits/useHabits";
import { useHabitCompletions } from "../features/habits/useHabitCompletions";
import { useHabitStats } from "../features/habits/useHabitStats";

export default function HabitDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const habitId = id ?? "";

  const today = formatLocalDate(new Date());
  const monthStart = `${today.slice(0, 7)}-01`;

  const { data: habit, isLoading: isHabitLoading, isError: isHabitError } = useHabit(habitId);
  const { data: stats, isLoading: isStatsLoading } = useHabitStats(habitId, today);
  const { data: completions, isLoading: isCompletionsLoading } = useHabitCompletions(
    habitId,
    monthStart,
    today,
  );

  if (isHabitLoading) {
    return <p className="page-status">Loading habit...</p>;
  }

  if (isHabitError || !habit) {
    return <p className="page-status page-error">Failed to load habit.</p>;
  }

  const streakLabel = habit.frequency === "Daily" ? "day streak" : "week streak";

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

      {isStatsLoading || !stats ? (
        <p className="page-status">Loading stats...</p>
      ) : (
        <div className="stats-panel">
          <div className="stat-block">
            <span className="stat-value">{stats.currentStreak}</span>
            <span className="stat-label">{streakLabel}</span>
          </div>
          <div className="stat-block">
            <span className="stat-value">{stats.monthlyCompletionRate}%</span>
            <span className="stat-label">this month</span>
          </div>
          <div className="stat-block">
            <span className="stat-value">
              {stats.completedThisMonth} / {stats.daysElapsedThisMonth}
            </span>
            <span className="stat-label">days completed</span>
          </div>
        </div>
      )}

      <h2>This month</h2>
      {isCompletionsLoading ? (
        <p className="page-status">Loading calendar...</p>
      ) : (
        <CompletionCalendar completions={completions ?? []} today={today} />
      )}
    </div>
  );
}
