"use client";

import type { ComponentType } from "react";
import { CalcForm } from "./CalcForm";
import { MultipleChoiceForm, OrderedResponseForm, SataForm, SelectNForm } from "./ChoiceForms";
import { DropdownClozeForm, DropdownTableForm } from "./DropdownForms";
import { GroupingForm } from "./GroupingForm";
import { HighlightTableForm, HighlightTextForm } from "./HighlightForms";
import { MatrixMcForm, MatrixMrForm } from "./MatrixForms";
import type { AuthorableType, ItemFormProps, ItemParts } from "./payloads";

type AnyForm = ComponentType<ItemFormProps<never>>;

/** The form for each MVP item type (interfaces.md 2.1 to 2.9). */
const forms: Record<AuthorableType, AnyForm> = {
  multiple_choice: MultipleChoiceForm as AnyForm,
  sata: SataForm as AnyForm,
  select_n: SelectNForm as AnyForm,
  ordered_response: OrderedResponseForm as AnyForm,
  matrix_mc: MatrixMcForm as AnyForm,
  matrix_mr: MatrixMrForm as AnyForm,
  multiple_response_grouping: GroupingForm as AnyForm,
  dropdown_cloze: DropdownClozeForm as AnyForm,
  dropdown_table: DropdownTableForm as AnyForm,
  highlight_text: HighlightTextForm as AnyForm,
  highlight_table: HighlightTableForm as AnyForm,
  fill_blank_calc: CalcForm as AnyForm,
};

interface ItemFormSwitchProps {
  itemType: AuthorableType;
  value: ItemParts;
  onChange(next: ItemParts): void;
  disabled: boolean;
}

/** Picks the content-and-key form for an item type; the casts only narrow to that type's shapes. */
export function ItemForm({ itemType, ...props }: ItemFormSwitchProps) {
  const Form = forms[itemType];
  return <Form {...(props as unknown as ItemFormProps<never>)} />;
}
