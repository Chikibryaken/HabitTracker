import type { DayOfWeek, HabitFrequency } from "../../types/habit";

const DAY_ORDER: DayOfWeek[] = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

const DAY_ABBREVIATIONS: Record<DayOfWeek, string> = {
  Monday: "Mon",
  Tuesday: "Tue",
  Wednesday: "Wed",
  Thursday: "Thu",
  Friday: "Fri",
  Saturday: "Sat",
  Sunday: "Sun",
};

export function getFrequencyBadgeClass(frequency: HabitFrequency): string {
  switch (frequency) {
    case "Daily":
      return "habit-badge-daily";
    case "Weekly":
      return "habit-badge-weekly";
    case "SpecificDays":
      return "habit-badge-specific-days";
    case "EveryOtherDay":
      return "habit-badge-every-other-day";
  }
}

export function getFrequencyLabel(frequency: HabitFrequency, daysOfWeek?: DayOfWeek[]): string {
  switch (frequency) {
    case "Daily":
      return "Daily";
    case "Weekly":
      return "Weekly";
    case "EveryOtherDay":
      return "Every other day";
    case "SpecificDays": {
      if (!daysOfWeek || daysOfWeek.length === 0) {
        return "Specific days";
      }
      return [...daysOfWeek]
        .sort((a, b) => DAY_ORDER.indexOf(a) - DAY_ORDER.indexOf(b))
        .map((day) => DAY_ABBREVIATIONS[day])
        .join(", ");
    }
  }
}
