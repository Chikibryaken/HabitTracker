import { useState } from "react";
import type { FormEvent } from "react";
import type { CreateHabitInput, HabitFrequency } from "../../types/habit";

interface HabitFormValues {
  name: string;
  description: string;
  frequency: HabitFrequency;
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
};

const MAX_NAME_LENGTH = 200;
const MAX_DESCRIPTION_LENGTH = 1000;

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
  const [error, setError] = useState<string | null>(null);

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

    setError(null);
    onSubmit({
      name: trimmedName,
      description: description.trim() || undefined,
      frequency,
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
      </select>

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
