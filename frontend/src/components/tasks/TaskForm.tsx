import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import './Task.css';

interface TaskFormProps {
  onSubmit: (title: string, dueDate?: string) => Promise<void>;
  onCancel: () => void;
}

const TaskSchema = Yup.object().shape({
  title: Yup.string()
    .required('Task title is required'),
  dueDate: Yup.date()
    .nullable()
    .min(new Date(), 'Due date cannot be in the past'),
});

const TaskForm: React.FC<TaskFormProps> = ({ onSubmit, onCancel }) => {
  const formik = useFormik({
    initialValues: {
      title: '',
      dueDate: '',
    },
    validationSchema: TaskSchema,
    onSubmit: async (values, { setSubmitting }) => {
      try {
        await onSubmit(values.title, values.dueDate || undefined);
      } finally {
        setSubmitting(false);
      }
    },
  });

  return (
    <div className="task-form">
      <form onSubmit={formik.handleSubmit}>
        <div className="form-group">
          <span>Title</span>
          <input
            id="title"
            type="text"
            placeholder="Task title *"
            {...formik.getFieldProps('title')}
            className={
              formik.touched.title && formik.errors.title
                ? 'input-error'
                : ''
            }
          />
          {formik.touched.title && formik.errors.title && (
            <div className="field-error">{formik.errors.title}</div>
          )}
        </div>

        <div className="form-group">
          <span>Due Date</span>
          <input
            id="dueDate"
            type="date"
            {...formik.getFieldProps('dueDate')}
            className={
              formik.touched.dueDate && formik.errors.dueDate
                ? 'input-error'
                : ''
            }
          />
          {formik.touched.dueDate && formik.errors.dueDate && (
            <div className="field-error">{formik.errors.dueDate}</div>
          )}
        </div>
          <div>
            <br />
        <div className="form-actions">
          <button
            type="button"
            onClick={onCancel}
            className="btn-secondary"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={formik.isSubmitting}
            className="btn-primary"
          >
            {formik.isSubmitting ? 'Adding...' : 'Add Task'}
          </button>
        </div>
        </div>
      </form>
    </div>
  );
};

export default TaskForm;
