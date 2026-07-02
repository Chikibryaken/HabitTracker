import { useState } from "react";
import type { MouseEvent } from "react";
import { useNavigate } from "react-router-dom";
import Modal from "../../components/Modal";
import type { Habit } from "../../types/habit";
import { formatLocalDate } from "./dateUtils";
import { getFrequencyBadgeClass, getFrequencyLabel } from "./frequencyLabels";
import HabitForm from "./HabitForm";
import { useArchiveHabit, useUpdateHabit } from "./useHabitMutations";
import { useHabitStats } from "./useHabitStats";
import { useTodayCompletion, useToggleCompletion } from "./useToggleCompletion";

interface HabitCardProps {
  habit: Habit;
}

export default function HabitCard({ habit }: HabitCardProps) {
  const navigate = useNavigate();
  const [isEditing, setIsEditing] = useState(false);

  const today = formatLocalDate(new Date());
  const { data: isCompletedToday, isLoading: isCompletionLoading } = useTodayCompletion(habit.id);
  const { data: stats } = useHabitStats(habit.id, today);
  const toggleCompletion = useToggleCompletion();
  const updateHabit = useUpdateHabit();
  const archiveHabit = useArchiveHabit();

  const handleCardClick = (): void => {
    navigate(`/habits/${habit.id}`);
  };

  const stopClickPropagation = (event: MouseEvent<HTMLElement>): void => {
    event.stopPropagation();
  };

  const handleToggleChange = (): void => {
    toggleCompletion.mutate({
      habitId: habit.id,
      date: today,
      isCurrentlyCompleted: Boolean(isCompletedToday),
    });
  };

  const handleArchive = (): void => {
    archiveHabit.mutate(habit.id);
  };

  return (
    <div className="habit-card" onClick={handleCardClick}>
      <input
        type="checkbox"
        className="habit-checkbox-input"
        aria-label={`Mark "${habit.name}" done today`}
        checked={Boolean(isCompletedToday)}
        disabled={isCompletionLoading || toggleCompletion.isPending}
        onClick={stopClickPropagation}
        onChange={handleToggleChange}
      />

      <div className="habit-info">
        <div className="habit-name-row">
          <span className="habit-name">{habit.name}</span>
          <span className={`habit-badge ${getFrequencyBadgeClass(habit.frequency)}`}>
            {getFrequencyLabel(habit.frequency, habit.daysOfWeek)}
          </span>
          {stats && stats.currentStreak > 0 && (
            <span className="streak-badge">🔥 {stats.currentStreak}</span>
          )}
        </div>
        {habit.description && <p className="habit-description">{habit.description}</p>}
      </div>

      <div className="habit-card-actions" onClick={stopClickPropagation}>
        <button type="button" onClick={() => setIsEditing(true)}>
          Edit
        </button>
        <button type="button" onClick={handleArchive} disabled={archiveHabit.isPending}>
          Archive
        </button>
      </div>

      {isEditing && (
        <Modal title="Edit habit" onClose={() => setIsEditing(false)}>
          <HabitForm
            initialValues={{
              name: habit.name,
              description: habit.description ?? "",
              frequency: habit.frequency,
              daysOfWeek: habit.daysOfWeek ?? [],
            }}
            submitLabel="Save"
            isSubmitting={updateHabit.isPending}
            onCancel={() => setIsEditing(false)}
            onSubmit={(input) => {
              updateHabit.mutate(
                { id: habit.id, input },
                { onSuccess: () => setIsEditing(false) },
              );
            }}
          />
        </Modal>
      )}
    </div>
  );
}
