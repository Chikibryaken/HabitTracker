import HabitCard from "./HabitCard";
import { useHabits } from "./useHabits";

export default function HabitList() {
  const { data: habits, isLoading, isError } = useHabits(false);

  if (isLoading) {
    return <p className="habit-list-status">Loading habits...</p>;
  }

  if (isError) {
    return <p className="habit-list-status habit-list-error">Failed to load habits. Please try again.</p>;
  }

  if (!habits || habits.length === 0) {
    return <p className="habit-list-status">No habits yet. Create your first one!</p>;
  }

  return (
    <div className="habit-list">
      {habits.map((habit) => (
        <HabitCard key={habit.id} habit={habit} />
      ))}
    </div>
  );
}
