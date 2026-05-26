export interface CrtcProject {
  readonly name: string;
  readonly problem: string;
  readonly tools: readonly string[];
  readonly actions: readonly string[];
  readonly result: string;
  readonly wcagScore: number; // 100
}