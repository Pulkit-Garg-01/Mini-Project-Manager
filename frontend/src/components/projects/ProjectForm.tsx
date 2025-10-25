import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import './Project.css';

interface ProjectFormProps {
  onSubmit: (title: string, description?: string) => Promise<void>;
  onCancel: () => void;
}

const ProjectSchema = Yup.object().shape({
  title: Yup.string()
    .min(3, 'Title must be at least 3 characters')
    .max(100, 'Title must not exceed 100 characters')
    .required('Title is required'),
  description: Yup.string()
    .max(500, 'Description must not exceed 500 characters'),
});

const ProjectForm: React.FC<ProjectFormProps> = ({ onSubmit, onCancel }) => {
  const formik = useFormik({
    initialValues: {
      title: '',
      description: '',
    },
    validationSchema: ProjectSchema,
    onSubmit: async (values, { setSubmitting }) => {
      try {
        await onSubmit(values.title, values.description || undefined);
      } finally {
        setSubmitting(false);
      }
    },
  });

  return (
    <div className="project-form">
      <h3>Create New Project</h3>
      <form onSubmit={formik.handleSubmit}>
        <div className="form-group">
          <label htmlFor="title">Title *</label>
          <input
            id="title"
            type="text"
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
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            rows={4}
            {...formik.getFieldProps('description')}
            className={
              formik.touched.description && formik.errors.description
                ? 'input-error'
                : ''
            }
          />
          {formik.touched.description && formik.errors.description && (
            <div className="field-error">{formik.errors.description}</div>
          )}
        </div>

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
            {formik.isSubmitting ? 'Creating...' : 'Create Project'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ProjectForm;
