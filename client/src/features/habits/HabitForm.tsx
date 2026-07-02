import { useState } from "react";
import type { FormEvent } from "react";
import type { CreateHabitInput, DayOfWeek, HabitFrequency } from "../../types/habit";

interface HabitFormValues {
  name: string;
  description: string;
  frequency: HabitFrequency;
  daysOfWeek: DayOfWeek[];
}

interface HabitFormProps {
  initialValues?: HabitFormValues;
  submitLabel: string;
  isSubmitting: boolean;
  onSubmit: (input: CreateHabitInput) => void;
  onCancel: () => void;
}

const DEFAULT_VALUES: HabitFormValues = {
  name: "",
  description: "",
  frequency: "Daily",
  daysOfWeek: [],
};

const MAX_NAME_LENGTH = 200;
const MAX_DESCRIPTION_LENGTH = 1000;

const DAY_OPTIONS: { value: DayOfWeek; label: string }[] = [
  { value: "Monday", label: "Mon" },
  { value: "Tuesday", label: "Tue" },
  { value: "Wednesday", label: "Wed" },
  { value: "Thursday", label: "Thu" },
  { value: "Friday", label: "Fri" },
  { value: "Saturday", label: "Sat" },
  { value: "Sunday", label: "Sun" },
];

export default function HabitForm({
  initialValues = DEFAULT_VALUES,
  submitLabel,
  isSubmitting,
  onSubmit,
  onCancel,
}: HabitFormProps) {
  const [name, setName] = useState(initialValues.name);
  const [description, setDescription] = useState(initialValues.description);
  const [frequency, setFrequency] = useState<HabitFrequency>(initialValues.frequency);
  const [daysOfWeek, setDaysOfWeek] = useState<DayOfWeek[]>(initialValues.daysOfWeek);
  const [error, setError] = useState<string | null>(null);

  const toggleDay = (day: DayOfWeek): void => {
    setDaysOfWeek((current) =>
      current.includes(day) ? current.filter((d) => d !== day) : [...current, day],
    );
  };

  const handleSubmit = (event: FormEvent<HTMLFormElement>): void => {
    event.preventDefault();

    const trimmedName = name.trim();
    if (!trimmedName) {
      setError("Name is required.");
      return;
    }
    if (trimmedName.length > MAX_NAME_LENGTH) {
      setError(`Name must be ${MAX_NAME_LENGTH} characters or fewer.`);
      return;
    }
    if (description.length > MAX_DESCRIPTION_LENGTH) {
      setError(`Description must be ${MAX_DESCRIPTION_LENGTH} characters or fewer.`);
      return;
    }
    if (frequency === "SpecificDays" && daysOfWeek.length === 0) {
      setError("Select at least one day of the week.");
      return;
    }

    setError(null);
    onSubmit({
      name: trimmedName,
      description: description.trim() || undefined,
      frequency,
      daysOfWeek: frequency === "SpecificDays" ? daysOfWeek : undefined,
    });
  };

  return (
    <form className="habit-form" onSubmit={handleSubmit} noValidate>
      <label htmlFor="habit-name">Name</label>
      <input
        id="habit-name"
        type="text"
        value={name}
        maxLength={MAX_NAME_LENGTH}
        onChange={(event) => setName(event.target.value)}
      />

      <label htmlFor="habit-description">Description</label>
      <textarea
        id="habit-description"
        value={description}
        maxLength={MAX_DESCRIPTION_LENGTH}
        rows={3}
        onChange={(event) => setDescription(event.target.value)}
      />

      <label htmlFor="habit-frequency">Frequency</label>
      <select
        id="habit-frequency"
        value={frequency}
        onChange={(event) => setFrequency(event.target.value as HabitFrequency)}
      >
        <option value="Daily">Daily</option>
        <option value="Weekly">Weekly</option>
        <option value="SpecificDays">Specific days</option>
        <option value="EveryOtherDay">Every other day</option>
      </select>

      {frequency === "SpecificDays" && (
        <div className="day-picker">
          {DAY_OPTIONS.map((day) => (
            <button
              key={day.value}
              type="button"
              className={`day-picker-option${daysOfWeek.includes(day.value) ? " day-picker-option-selected" : ""}`}
              onClick={() => toggleDay(day.value)}
              aria-pressed={daysOfWeek.includes(day.value)}
            >
              {day.label}
            </button>
          ))}
        </div>
      )}

      {error && <p className="form-error">{error}</p>}

      <div className="habit-form-actions">
        <button type="button" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </button>
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : submitLabel}
        </button>
      </div>
    </form>
  );
}
